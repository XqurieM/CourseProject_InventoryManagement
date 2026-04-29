using CourseProject_InventoryManagement.Domain.Common;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class RefreshToken : AuditableEntity
    {
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = null!;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public bool IsRevoked => RevokedAtUtc.HasValue;
    }
}
