namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class AuthTokenDto
    {
        public string AccessToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAtUtc { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTime RefreshTokenExpiresAtUtc { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsAdmin { get; set; }
    }
}
