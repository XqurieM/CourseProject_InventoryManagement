using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CourseProject_InventoryManagement.Infrastructure.Authentication
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;
        private readonly IRefreshTokenService _refreshTokenService;

        public JwtTokenService(IOptions<JwtSettings> options, IRefreshTokenService refreshTokenService)
        {
            _settings = options.Value;
            _refreshTokenService = refreshTokenService;
        }

        public AuthTokenDto CreateToken(AppUser user)
        {
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);
            var refreshToken = _refreshTokenService.GenerateToken();
            var refreshTokenExpiresAtUtc = _refreshTokenService.GetRefreshTokenExpiryUtc();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new AuthTokenDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                AccessTokenExpiresAtUtc = expiresAtUtc,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc,
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsAdmin = user.IsAdmin
            };
        }
    }
}
