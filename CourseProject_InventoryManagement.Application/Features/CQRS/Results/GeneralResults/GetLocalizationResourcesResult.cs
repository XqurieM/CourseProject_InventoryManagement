namespace CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults
{
    public class GetLocalizationResourcesResult
    {
        public string LanguageCode { get; set; } = string.Empty;
        public string? PageName { get; set; }
        public Dictionary<string, string> Resources { get; set; } = new();
    }
}
