using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class DeleteInventoryCommandHandler : ICQRS.IDeleteInventory
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public DeleteInventoryCommandHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService, IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> DeleteInventory(DeleteInventoryCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken);

            if (inventory is null)
            {
                return Result<Guid>.NotFound("Inventory not found.");
            }

            var canManage = await _inventoryAuthorizationService
                .CanManageInventoryAsync(command.Id, userResult.Value.Id, cancellationToken);

            if (!canManage)
            {
                return Result<Guid>.Forbidden("You do not have permission to delete this inventory.");
            }

            var utcNow = DateTime.UtcNow;

            inventory.IsDeleted = true;
            inventory.DeletedAtUtc = utcNow;
            inventory.UpdatedAtUtc = utcNow;
            inventory.UpdatedByUserId = userResult.Value.Id;

            var inventoryItems = await _context.Items
                .Where(x => x.InventoryId == inventory.Id && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var item in inventoryItems)
            {
                item.IsDeleted = true;
                item.DeletedAtUtc = utcNow;
                item.UpdatedAtUtc = utcNow;
                item.UpdatedByUserId = userResult.Value.Id;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(inventory.Id);
        }

    }
}
