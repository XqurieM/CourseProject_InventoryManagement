using CourseProject_InventoryManagement.Domain.Entities;

namespace CourseProject_InventoryManagement.Application.Abstractions.Authentication
{
    public interface IRefreshTokenService
    {
        string GenerateToken();
        string ComputeHash(string token);
        DateTime GetRefreshTokenExpiryUtc();
        RefreshToken CreateRefreshToken(AppUser user, string rawToken);
    }
}
