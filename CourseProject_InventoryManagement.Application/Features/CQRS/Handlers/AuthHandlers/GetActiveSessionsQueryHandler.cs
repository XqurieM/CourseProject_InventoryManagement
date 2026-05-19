using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.AuthQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.AuthHandlers
{
    public class GetActiveSessionsQueryHandler : ICQRS.IGetActiveSessions
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetActiveSessionsQueryHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Result<List<ActiveSessionDto>>> GetActiveSessions(GetActiveSessionsQuery query, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<ActiveSessionDto>>(userResult);
            }

            var sessions = await _context.RefreshTokens
                .Where(x => x.UserId == userResult.Value.Id)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new ActiveSessionDto
                {
                    Id = x.Id,
                    CreatedAtUtc = x.CreatedAtUtc,
                    UpdatedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
                    ExpiresAtUtc = x.ExpiresAtUtc,
                    IsRevoked = x.IsRevoked,
                    RevokedAtUtc = x.RevokedAtUtc,
                    TokenPreview = x.TokenHash.Length >= 8 ? x.TokenHash.Substring(0, 8) : x.TokenHash
                })
                .ToListAsync(cancellationToken);

            return Result<List<ActiveSessionDto>>.Success(sessions);
        }
    }
}
