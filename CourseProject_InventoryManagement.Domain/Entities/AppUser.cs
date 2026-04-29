using CourseProject_InventoryManagement.Domain.Common;
using CourseProject_InventoryManagement.Domain.Enums;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class AppUser : SoftDeletableEntity
    {
        public string UserName { get; set; } = null!;
        public string NormalizedUserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string NormalizedEmail { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsAdmin { get; set; }
        public bool IsBlocked { get; set; }
        public LanguageType PreferredLanguage { get; set; } = LanguageType.English;
        public ThemeType PreferredTheme { get; set; } = ThemeType.Dark;
    }
}
