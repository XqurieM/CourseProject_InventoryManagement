namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CommentCommands
{
    public class UpdateCommentCommand
    {
        public Guid CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
