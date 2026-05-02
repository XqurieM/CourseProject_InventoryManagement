using CourseProject_InventoryManagement.Application.Abstractions.Localization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CourseProject_InventoryManagement.Infrastructure.Localization
{
    public class LocalizationCacheService : ILocalizationCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IAppDbContext _context;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public LocalizationCacheService(IMemoryCache memoryCache, IAppDbContext context)
        {
            _memoryCache = memoryCache;
            _context = context;
        }

        public async Task<List<LocalizationResourceAdminResult>> GetActiveResourcesAsync(string languageCode, CancellationToken cancellationToken = default)
        {
            var cacheKey = BuildCacheKey(languageCode);
            if (_memoryCache.TryGetValue(cacheKey, out List<LocalizationResourceAdminResult>? cached) && cached is not null)
            {
                return cached;
            }

            var resources = await _context.LocalizationResources
                .AsNoTracking()
                .Where(x => x.LanguageCode == languageCode && x.IsActive)
                .OrderBy(x => x.PageName)
                .ThenBy(x => x.ResourceKey)
                .Select(x => new LocalizationResourceAdminResult
                {
                    Id = x.Id,
                    ResourceKey = x.ResourceKey,
                    LanguageCode = x.LanguageCode,
                    Value = x.Value,
                    IsActive = x.IsActive,
                    PageName = x.PageName,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId,
                    UpdatedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
                    UpdatedByUserId = x.UpdatedByUserId ?? x.CreatedByUserId
                })
                .ToListAsync(cancellationToken);

            _memoryCache.Set(cacheKey, resources, CacheDuration);
            return resources;
        }

        public void Invalidate(string languageCode)
        {
            _memoryCache.Remove(BuildCacheKey(languageCode));
        }

        public void InvalidateMany(IEnumerable<string> languageCodes)
        {
            foreach (var languageCode in languageCodes.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                Invalidate(languageCode);
            }
        }

        public void InvalidateAll()
        {
            foreach (var languageCode in new[] { "tr", "en", "de", "fr", "es", "it", "ru", "zh" })
            {
                Invalidate(languageCode);
            }
        }

        private static string BuildCacheKey(string languageCode) => $"localization:{languageCode.Trim().ToLowerInvariant()}";
    }
}
