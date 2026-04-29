using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Infrastructure.Authorization
{
    public class InventoryAuthorizationService : IInventoryAuthorizationService
    {
        private readonly IAppDbContext _context;

        public InventoryAuthorizationService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanManageInventoryAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);

            if (user is null || user.IsBlocked)
            {
                return false;
            }

            if (user.IsAdmin)
            {
                return true;
            }

            return await _context.Inventories
                .AsNoTracking()
                .AnyAsync(x => x.Id == inventoryId && x.CreatedByUserId == userId, cancellationToken);
        }

        public async Task<bool> CanWriteItemsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);

            if (user is null || user.IsBlocked)
            {
                return false;
            }

            if (user.IsAdmin)
            {
                return true;
            }

            var inventory = await _context.Inventories
                .AsNoTracking()
                .Where(x => x.Id == inventoryId)
                .Select(x => new
                {
                    x.CreatedByUserId,
                    x.IsPublic
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (inventory is null)
            {
                return false;
            }

            if (inventory.CreatedByUserId == userId || inventory.IsPublic)
            {
                return true;
            }

            return await _context.InventoryAccesses
                .AsNoTracking()
                .AnyAsync(x => x.InventoryId == inventoryId && x.UserId == userId, cancellationToken);
        }
    }
}
