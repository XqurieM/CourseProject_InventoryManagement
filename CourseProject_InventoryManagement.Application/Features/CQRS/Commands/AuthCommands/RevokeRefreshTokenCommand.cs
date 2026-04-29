namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands
{
    public class RevokeRefreshTokenCommand
    {
        public string RefreshToken { get; set; } = null!;
    }
}
