namespace CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries
{
    public class GetLocalizationResourcesQuery
    {
        public string? LanguageCode { get; set; }
        public string? PageName { get; set; }
        public bool ActiveOnly { get; set; } = true;
    }
}
