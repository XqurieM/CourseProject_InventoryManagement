using CourseProject_InventoryManagement.Domain.Enums;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults
{
    public class GetInventoryCustomIdRulesByInventoryIdResult
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public int PartOrder { get; set; }
        public CustomIdPartType PartType { get; set; }
        public string? Format { get; set; }
        public string? StaticTextValue { get; set; }
    }
}
