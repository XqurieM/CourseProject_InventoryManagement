namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands
{
    public class RegisterUserCommand
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
