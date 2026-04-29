namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands
{
    public class LoginUserCommand
    {
        public string EmailOrUserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
