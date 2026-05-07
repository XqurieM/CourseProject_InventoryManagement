using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Infrastructure.Authorization
{
    public class AdministrationAuthorizationService : IAdministrationAuthorizationService
    {
        private readonly IAppDbContext _context;

        public AdministrationAuthorizationService(IAppDbContext context)
        {
            _context = context;
        }

        public Task<bool> CanManageUsersAsync(Guid userId, CancellationToken cancellationToken = default) =>
            IsActiveAdminAsync(userId, cancellationToken);

        public Task<bool> CanManageLocalizationAsync(Guid userId, CancellationToken cancellationToken = default) =>
            IsActiveAdminAsync(userId, cancellationToken);

        private Task<bool> IsActiveAdminAsync(Guid userId, CancellationToken cancellationToken) =>
            _context.Users
                .AsNoTracking()
                .AnyAsync(x => x.Id == userId && !x.IsDeleted && !x.IsBlocked && x.IsAdmin, cancellationToken);
    }
}
