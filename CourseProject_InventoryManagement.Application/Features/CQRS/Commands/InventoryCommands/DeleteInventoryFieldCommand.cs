namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class DeleteInventoryFieldCommand
    {
        public Guid InventoryId { get; set; }
        public Guid FieldId { get; set; }
    }
}
