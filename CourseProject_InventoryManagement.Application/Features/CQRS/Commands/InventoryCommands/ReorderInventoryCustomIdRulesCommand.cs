using CourseProject_InventoryManagement.Application.DTOs;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class ReorderInventoryCustomIdRulesCommand
    {
        public Guid InventoryId { get; set; }
        public List<ReorderInventoryCustomIdRuleDto> Rules { get; set; } = new();
    }
}
