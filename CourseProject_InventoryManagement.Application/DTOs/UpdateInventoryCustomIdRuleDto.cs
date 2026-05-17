using CourseProject_InventoryManagement.Domain.Enums;

namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class UpdateInventoryCustomIdRuleDto
    {
        public Guid Id { get; set; }
        public int PartOrder { get; set; }
        public CustomIdPartType PartType { get; set; }
        public string? Format { get; set; }
        public string? StaticTextValue { get; set; }
    }
}
