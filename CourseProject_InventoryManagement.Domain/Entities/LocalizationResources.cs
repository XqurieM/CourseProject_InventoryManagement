using CourseProject_InventoryManagement.Domain.Common;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class LocalizationResources : AuditableEntity
    {
        public string ResourceKey { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string PageName { get; set; } = string.Empty;
    }
}
