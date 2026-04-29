using CourseProject_InventoryManagement.Domain.Enums;

namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsAdmin { get; set; }
        public bool IsBlocked { get; set; }
        public LanguageType PreferredLanguage { get; set; }
        public ThemeType PreferredTheme { get; set; }
    }
}
