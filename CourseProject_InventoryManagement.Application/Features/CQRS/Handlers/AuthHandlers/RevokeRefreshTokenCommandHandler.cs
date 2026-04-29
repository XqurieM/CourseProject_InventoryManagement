using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.AuthHandlers
{
    public class RevokeRefreshTokenCommandHandler : ICQRS.IRevokeRefreshToken
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IRefreshTokenService _refreshTokenService;

        public RevokeRefreshTokenCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IRefreshTokenService refreshTokenService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<Result> RevokeRefreshToken(RevokeRefreshTokenCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return Result.Unauthorized(userResult.Errors.ToArray());
            }

            if (string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                return Result.Invalid(new ValidationError(nameof(command.RefreshToken), "Refresh token is required."));
            }

            var tokenHash = _refreshTokenService.ComputeHash(command.RefreshToken.Trim());

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.UserId == userResult.Value.Id, cancellationToken);

            if (refreshToken is null)
            {
                return Result.NotFound("Refresh token was not found.");
            }

            if (!refreshToken.IsRevoked)
            {
                refreshToken.RevokedAtUtc = DateTime.UtcNow;
                refreshToken.UpdatedByUserId = userResult.Value.Id;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Result.Success();
        }
    }
}
