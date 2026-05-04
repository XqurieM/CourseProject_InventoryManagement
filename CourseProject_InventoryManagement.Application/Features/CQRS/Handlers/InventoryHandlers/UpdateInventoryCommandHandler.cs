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
    public class UpdateInventoryCommandHandler : ICQRS.IUpdateInventory
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;
        public UpdateInventoryCommandHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService, IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> UpdateInventory(UpdateInventoryCommand command, CancellationToken cancellationToken = default)
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
                return Result<Guid>.Forbidden("You do not have permission to update this inventory.");
            }

            if (command.RowVersion is null || command.RowVersion.Length == 0)
            {
                return Result<Guid>.Invalid(new ValidationError(nameof(command.RowVersion), "RowVersion is required."));
            }

            if (command.Title is not null)
            {
                if (string.IsNullOrWhiteSpace(command.Title))
                {
                    return Result<Guid>.Invalid(new ValidationError(nameof(command.Title), "Title cannot be empty."));
                }

                inventory.Title = command.Title.Trim();
            }

            if (command.Description is not null)
            {
                inventory.Description = command.Description.Trim();
            }

            if (command.ImageUrl is not null)
            {
                inventory.ImageUrl = command.ImageUrl.Trim();
            }

            if (command.IsPublic.HasValue)
            {
                inventory.IsPublic = command.IsPublic.Value;
            }

            if (command.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories
                    .AnyAsync(x => x.Id == command.CategoryId.Value && x.IsActive, cancellationToken);

                if (!categoryExists)
                {
                    return Result<Guid>.NotFound("Category not found.");
                }

                inventory.CategoryId = command.CategoryId.Value;
            }

            inventory.UpdatedAtUtc = DateTime.UtcNow;
            inventory.UpdatedByUserId = userResult.Value.Id;
            _context.Entry(inventory).Property(x => x.RowVersion).OriginalValue = command.RowVersion;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<Guid>.Conflict("The inventory was updated by another user. Please refresh and try again.");
            }

            return Result<Guid>.Success(inventory.Id);
        }

    }
}
