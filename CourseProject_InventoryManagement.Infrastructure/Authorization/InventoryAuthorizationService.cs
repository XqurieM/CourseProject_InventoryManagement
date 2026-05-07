using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Infrastructure.Authorization
{
    public class InventoryAuthorizationService : IInventoryAuthorizationService
    {
        private sealed class ActiveUserInfo
        {
            public Guid Id { get; init; }
            public bool IsAdmin { get; init; }
            public bool IsBlocked { get; init; }
        }

        private readonly IAppDbContext _context;

        public InventoryAuthorizationService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanViewInventoryAsync(Guid inventoryId, Guid? userId, CancellationToken cancellationToken = default)
        {
            var inventory = await _context.Inventories
                .AsNoTracking()
                .Where(x => x.Id == inventoryId && !x.IsDeleted)
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

            if (inventory.IsPublic)
            {
                return true;
            }

            if (!userId.HasValue)
            {
                return false;
            }

            var user = await GetActiveUserAsync(userId.Value, cancellationToken);
            if (user is null)
            {
                return false;
            }

            if (user.IsAdmin || inventory.CreatedByUserId == userId.Value)
            {
                return true;
            }

            return await _context.InventoryAccesses
                .AsNoTracking()
                .AnyAsync(x => x.InventoryId == inventoryId && x.UserId == userId.Value, cancellationToken);
        }

        public async Task<bool> CanManageInventoryAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await GetActiveUserAsync(userId, cancellationToken);
            if (user is null)
            {
                return false;
            }

            if (user.IsAdmin)
            {
                return true;
            }

            return await _context.Inventories
                .AsNoTracking()
                .AnyAsync(x => x.Id == inventoryId && !x.IsDeleted && x.CreatedByUserId == userId, cancellationToken);
        }

        public Task<bool> CanManageInventoryFieldsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default) =>
            CanManageInventoryAsync(inventoryId, userId, cancellationToken);

        public Task<bool> CanManageInventoryCustomIdRulesAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default) =>
            CanManageInventoryAsync(inventoryId, userId, cancellationToken);

        public Task<bool> CanManageInventoryAccessAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default) =>
            CanManageInventoryAsync(inventoryId, userId, cancellationToken);

        public Task<bool> CanViewInventoryAccessListAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default) =>
            CanManageInventoryAccessAsync(inventoryId, userId, cancellationToken);

        public Task<bool> CanCreateItemsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default) =>
            CanWriteItemsAsync(inventoryId, userId, cancellationToken);

        public Task<bool> CanUpdateItemsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default) =>
            CanWriteItemsAsync(inventoryId, userId, cancellationToken);

        public Task<bool> CanDeleteItemsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default) =>
            CanWriteItemsAsync(inventoryId, userId, cancellationToken);

        public async Task<bool> CanCommentOnInventoryAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await GetActiveUserAsync(userId, cancellationToken);
            if (user is null)
            {
                return false;
            }

            return await CanViewInventoryAsync(inventoryId, userId, cancellationToken);
        }

        public async Task<bool> CanWriteItemsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await GetActiveUserAsync(userId, cancellationToken);
            if (user is null)
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

        private async Task<ActiveUserInfo?> GetActiveUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == userId && !x.IsDeleted)
                .Select(x => new ActiveUserInfo
                {
                    Id = x.Id,
                    IsAdmin = x.IsAdmin,
                    IsBlocked = x.IsBlocked
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null || user.IsBlocked)
            {
                return null;
            }

            return user;
        }
    }
}
