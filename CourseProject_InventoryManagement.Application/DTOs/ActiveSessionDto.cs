namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class ActiveSessionDto
    {
        public Guid Id { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public string TokenPreview { get; set; } = string.Empty;
    }
}
