using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace CourseProject_InventoryManagement.Infrastructure.Authentication
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly JwtSettings _settings;

        public RefreshTokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public string GenerateToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public string ComputeHash(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }

        public DateTime GetRefreshTokenExpiryUtc()
        {
            return DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
        }

        public RefreshToken CreateRefreshToken(AppUser user, string rawToken)
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = ComputeHash(rawToken),
                ExpiresAtUtc = GetRefreshTokenExpiryUtc(),
                CreatedByUserId = user.Id
            };
        }
    }
}
