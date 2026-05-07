using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Infrastructure.Authorization
{
    public class UserAuthorizationService : IUserAuthorizationService
    {
        private readonly IAppDbContext _context;

        public UserAuthorizationService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanAccessUserScopedDataAsync(Guid requestedUserId, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            if (requestedUserId == currentUserId)
            {
                return await _context.Users
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == currentUserId && !x.IsDeleted && !x.IsBlocked, cancellationToken);
            }

            return await _context.Users
                .AsNoTracking()
                .AnyAsync(x => x.Id == currentUserId && !x.IsDeleted && !x.IsBlocked && x.IsAdmin, cancellationToken);
        }
    }
}
