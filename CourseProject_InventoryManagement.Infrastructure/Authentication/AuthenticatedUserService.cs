using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Infrastructure.Authentication
{
    public class AuthenticatedUserService : IAuthenticatedUserService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppDbContext _context;

        public AuthenticatedUserService(ICurrentUserService currentUserService, IAppDbContext context)
        {
            _currentUserService = currentUserService;
            _context = context;
        }

        public async Task<Result<AppUser>> GetRequiredUserAsync(CancellationToken cancellationToken = default)
        {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            {
                return Result<AppUser>.Unauthorized("Authentication is required.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == _currentUserService.UserId.Value && !x.IsDeleted, cancellationToken);

            if (user is null)
            {
                return Result<AppUser>.Unauthorized("Authenticated user was not found.");
            }

            if (user.IsBlocked)
            {
                return Result<AppUser>.Forbidden("Your account is blocked.");
            }

            return Result<AppUser>.Success(user);
        }

        public async Task<Result<AppUser>> GetRequiredAdminAsync(CancellationToken cancellationToken = default)
        {
            var userResult = await GetRequiredUserAsync(cancellationToken);

            if (!userResult.IsSuccess)
            {
                return userResult;
            }

            if (!userResult.Value.IsAdmin)
            {
                return Result<AppUser>.Forbidden("Admin permission is required.");
            }

            return userResult;
        }
    }
}
