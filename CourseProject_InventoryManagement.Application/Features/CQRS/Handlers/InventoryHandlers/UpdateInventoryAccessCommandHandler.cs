using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class UpdateInventoryAccessCommandHandler : ICQRS.IUpdateInventoryAccess
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public UpdateInventoryAccessCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> UpdateInventoryAccess(UpdateInventoryAccessCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var currentUser = userResult.Value;
            var canManage = await _inventoryAuthorizationService.CanManageInventoryAsync(command.InventoryId, currentUser.Id, cancellationToken);
            if (!canManage)
            {
                return Result<Guid>.Forbidden("Only the inventory owner or an admin can manage inventory access.");
            }

            var inventory = await _context.Inventories.FirstOrDefaultAsync(x => x.Id == command.InventoryId, cancellationToken);
            if (inventory is null)
            {
                return Result<Guid>.NotFound("Inventory not found.");
            }

            var distinctUserIds = command.UserIds
                .Where(x => x != Guid.Empty && x != currentUser.Id)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(x => distinctUserIds.Contains(x.Id) && !x.IsDeleted && !x.IsBlocked)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (users.Count != distinctUserIds.Count)
            {
                return Result<Guid>.Invalid(new ValidationError(nameof(command.UserIds), "One or more selected users are invalid, deleted, or blocked."));
            }

            inventory.IsPublic = command.IsPublic;

            var existingAccesses = await _context.InventoryAccesses
                .Where(x => x.InventoryId == command.InventoryId)
                .ToListAsync(cancellationToken);

            if (existingAccesses.Count > 0)
            {
                _context.InventoryAccesses.RemoveRange(existingAccesses);
            }

            if (!command.IsPublic && users.Count > 0)
            {
                var newAccesses = users.Select(userId => new InventoryAccess
                {
                    Id = Guid.NewGuid(),
                    InventoryId = command.InventoryId,
                    UserId = userId,
                    CreatedByUserId = currentUser.Id
                }).ToList();

                await _context.InventoryAccesses.AddRangeAsync(newAccesses, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(command.InventoryId);
        }
    }
}
