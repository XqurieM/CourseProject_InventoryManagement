using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Localization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.GeneralHandlers
{
    public class GetLocalizationResourcesQueryHandler : ICQRS.IGetLocalizationResources
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppDbContext _context;
        private readonly ILocalizationCacheService _localizationCacheService;

        public GetLocalizationResourcesQueryHandler(
            ICurrentUserService currentUserService,
            IAppDbContext context,
            ILocalizationCacheService localizationCacheService)
        {
            _currentUserService = currentUserService;
            _context = context;
            _localizationCacheService = localizationCacheService;
        }

        public async Task<Result<GetLocalizationResourcesResult>> GetLocalizationResources(GetLocalizationResourcesQuery query, CancellationToken cancellationToken = default)
        {
            var preferredLanguage = await ResolveCurrentUserLanguageAsync(cancellationToken);
            var languageCode = LocalizationLanguageCodeHelper.NormalizeOrDefault(query.LanguageCode, preferredLanguage);
            var pageName = string.IsNullOrWhiteSpace(query.PageName) ? null : query.PageName.Trim();

            if (!query.ActiveOnly)
            {
                var dbQuery = _context.LocalizationResources
                    .AsNoTracking()
                    .Where(x => x.LanguageCode == languageCode);

                if (!string.IsNullOrWhiteSpace(pageName))
                {
                    dbQuery = dbQuery.Where(x => x.PageName == pageName);
                }

                var allResources = await dbQuery
                    .OrderBy(x => x.PageName)
                    .ThenBy(x => x.ResourceKey)
                    .ToDictionaryAsync(x => x.ResourceKey, x => x.Value, cancellationToken);

                return Result<GetLocalizationResourcesResult>.Success(new GetLocalizationResourcesResult
                {
                    LanguageCode = languageCode,
                    PageName = pageName,
                    Resources = allResources
                });
            }

            var cachedResources = await _localizationCacheService.GetActiveResourcesAsync(languageCode, cancellationToken);
            var filteredResources = cachedResources
                .Where(x => string.IsNullOrWhiteSpace(pageName) || x.PageName == pageName)
                .OrderBy(x => x.PageName)
                .ThenBy(x => x.ResourceKey)
                .ToDictionary(x => x.ResourceKey, x => x.Value);

            return Result<GetLocalizationResourcesResult>.Success(new GetLocalizationResourcesResult
            {
                LanguageCode = languageCode,
                PageName = pageName,
                Resources = filteredResources
            });
        }

        private async Task<Domain.Enums.LanguageType?> ResolveCurrentUserLanguageAsync(CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return null;
            }

            var currentUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == _currentUserService.UserId.Value && !x.IsDeleted, cancellationToken);

            return currentUser?.PreferredLanguage;
        }
    }
}
