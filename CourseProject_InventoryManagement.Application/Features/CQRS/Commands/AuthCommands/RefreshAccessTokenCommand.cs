namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands
{
    public class RefreshAccessTokenCommand
    {
        public string RefreshToken { get; set; } = null!;
    }
}
