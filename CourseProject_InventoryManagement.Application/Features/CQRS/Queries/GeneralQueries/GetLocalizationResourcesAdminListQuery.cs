namespace CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries
{
    public class GetLocalizationResourcesAdminListQuery
    {
        public string? LanguageCode { get; set; }
        public string? PageName { get; set; }
        public string? ResourceKey { get; set; }
        public bool? IsActive { get; set; }
    }
}
