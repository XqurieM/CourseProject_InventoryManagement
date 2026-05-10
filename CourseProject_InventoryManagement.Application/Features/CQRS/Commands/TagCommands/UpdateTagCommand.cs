namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.TagCommands
{
    public class UpdateTagCommand
    {
        public Guid TagId { get; set; }
        public string? Name { get; set; }
    }
}
