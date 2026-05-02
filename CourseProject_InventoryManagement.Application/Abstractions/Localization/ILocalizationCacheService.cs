using CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults;

namespace CourseProject_InventoryManagement.Application.Abstractions.Localization
{
    public interface ILocalizationCacheService
    {
        Task<List<LocalizationResourceAdminResult>> GetActiveResourcesAsync(string languageCode, CancellationToken cancellationToken = default);
        void Invalidate(string languageCode);
        void InvalidateMany(IEnumerable<string> languageCodes);
        void InvalidateAll();
    }
}
