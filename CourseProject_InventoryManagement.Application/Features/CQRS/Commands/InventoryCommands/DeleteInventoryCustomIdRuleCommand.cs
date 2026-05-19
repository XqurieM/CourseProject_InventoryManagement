namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class DeleteInventoryCustomIdRuleCommand
    {
        public Guid InventoryId { get; set; }
        public Guid RuleId { get; set; }
    }
}
