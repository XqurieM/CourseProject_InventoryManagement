using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.AuthHandlers
{
    public class RefreshAccessTokenCommandHandler : ICQRS.IRefreshAccessToken
    {
        private readonly IAppDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public RefreshAccessTokenCommandHandler(
            IAppDbContext context,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<Result<AuthTokenDto>> RefreshAccessToken(RefreshAccessTokenCommand command, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                return Result<AuthTokenDto>.Invalid(new ValidationError(nameof(command.RefreshToken), "Refresh token is required."));
            }

            var tokenHash = _refreshTokenService.ComputeHash(command.RefreshToken.Trim());

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

            if (refreshToken is null || refreshToken.IsRevoked || refreshToken.ExpiresAtUtc <= DateTime.UtcNow)
            {
                return Result<AuthTokenDto>.Unauthorized("Refresh token is invalid or expired.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == refreshToken.UserId && !x.IsDeleted, cancellationToken);

            if (user is null)
            {
                return Result<AuthTokenDto>.Unauthorized("User was not found.");
            }

            if (user.IsBlocked)
            {
                return Result<AuthTokenDto>.Forbidden("Your account is blocked.");
            }

            var authToken = _jwtTokenService.CreateToken(user);
            var newRefreshToken = _refreshTokenService.CreateRefreshToken(user, authToken.RefreshToken);

            refreshToken.RevokedAtUtc = DateTime.UtcNow;
            refreshToken.ReplacedByTokenHash = newRefreshToken.TokenHash;
            refreshToken.UpdatedByUserId = user.Id;

            await _context.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<AuthTokenDto>.Success(authToken);
        }
    }
}
