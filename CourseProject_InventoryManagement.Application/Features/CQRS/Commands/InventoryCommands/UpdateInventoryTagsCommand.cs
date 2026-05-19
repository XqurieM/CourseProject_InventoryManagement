namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class UpdateInventoryTagsCommand
    {
        public Guid InventoryId { get; set; }
        public List<Guid> TagIds { get; set; } = new();
    }
}
