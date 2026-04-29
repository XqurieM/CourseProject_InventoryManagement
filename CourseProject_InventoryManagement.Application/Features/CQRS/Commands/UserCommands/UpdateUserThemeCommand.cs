using CourseProject_InventoryManagement.Domain.Enums;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands
{
    public class UpdateUserThemeCommand
    {
        public ThemeType PreferredTheme { get; set; }
    }
}
