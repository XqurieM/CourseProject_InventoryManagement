namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class UpdateInventoryAccessCommand
    {
        public Guid InventoryId { get; set; }
        public bool IsPublic { get; set; }
        public List<Guid> UserIds { get; set; } = new();
    }
}
