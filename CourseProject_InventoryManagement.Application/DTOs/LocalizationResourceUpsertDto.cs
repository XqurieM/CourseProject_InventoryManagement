namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class LocalizationResourceUpsertDto
    {
        public Guid? Id { get; set; }
        public string ResourceKey { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string PageName { get; set; } = string.Empty;
    }
}
