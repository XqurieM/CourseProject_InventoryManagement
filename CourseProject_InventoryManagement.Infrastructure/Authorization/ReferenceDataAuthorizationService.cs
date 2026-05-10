using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Infrastructure.Authorization
{
    public class ReferenceDataAuthorizationService : IReferenceDataAuthorizationService
    {
        private readonly IAppDbContext _context;

        public ReferenceDataAuthorizationService(IAppDbContext context)
        {
            _context = context;
        }

        public Task<bool> CanReadCategoriesAsync(Guid userId, CancellationToken cancellationToken = default) =>
            IsActiveUserAsync(userId, cancellationToken);

        public Task<bool> CanManageCategoriesAsync(Guid userId, CancellationToken cancellationToken = default) =>
            IsActiveAdminAsync(userId, cancellationToken);

        public Task<bool> CanReadTagsAsync(Guid userId, CancellationToken cancellationToken = default) =>
            IsActiveUserAsync(userId, cancellationToken);

        public Task<bool> CanManageTagsAsync(Guid userId, CancellationToken cancellationToken = default) =>
            IsActiveAdminAsync(userId, cancellationToken);

        private Task<bool> IsActiveUserAsync(Guid userId, CancellationToken cancellationToken) =>
            _context.Users
                .AsNoTracking()
                .AnyAsync(x => x.Id == userId && !x.IsDeleted && !x.IsBlocked, cancellationToken);

        private Task<bool> IsActiveAdminAsync(Guid userId, CancellationToken cancellationToken) =>
            _context.Users
                .AsNoTracking()
                .AnyAsync(x => x.Id == userId && !x.IsDeleted && !x.IsBlocked && x.IsAdmin, cancellationToken);
    }
}
