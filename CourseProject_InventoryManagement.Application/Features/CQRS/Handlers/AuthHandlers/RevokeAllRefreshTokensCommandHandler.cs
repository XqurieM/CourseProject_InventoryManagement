using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.AuthHandlers
{
    public class RevokeAllRefreshTokensCommandHandler : ICQRS.IRevokeAllRefreshTokens
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IRefreshTokenService _refreshTokenService;

        public RevokeAllRefreshTokensCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IRefreshTokenService refreshTokenService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<Result<int>> RevokeAllRefreshTokens(RevokeAllRefreshTokensCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, int>(userResult);
            }

            var currentRefreshTokenHash = string.IsNullOrWhiteSpace(command.CurrentRefreshToken)
                ? null
                : _refreshTokenService.ComputeHash(command.CurrentRefreshToken);

            var tokens = await _context.RefreshTokens
                .Where(x =>
                    x.UserId == userResult.Value.Id &&
                    !x.IsRevoked &&
                    (currentRefreshTokenHash == null || x.TokenHash != currentRefreshTokenHash))
                .ToListAsync(cancellationToken);

            var utcNow = DateTime.UtcNow;
            foreach (var token in tokens)
            {
                token.RevokedAtUtc = utcNow;
                token.UpdatedAtUtc = utcNow;
                token.UpdatedByUserId = userResult.Value.Id;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(tokens.Count);
        }
    }
}
