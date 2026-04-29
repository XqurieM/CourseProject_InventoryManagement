using CourseProject_InventoryManagement.Domain.Enums;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands
{
    public class UpdateUserLanguageCommand
    {
        public LanguageType PreferredLanguage { get; set; }
    }
}
