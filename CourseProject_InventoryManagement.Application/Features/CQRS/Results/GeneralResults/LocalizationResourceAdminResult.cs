namespace CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults
{
    public class LocalizationResourceAdminResult
    {
        public Guid Id { get; set; }
        public string ResourceKey { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string PageName { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}
