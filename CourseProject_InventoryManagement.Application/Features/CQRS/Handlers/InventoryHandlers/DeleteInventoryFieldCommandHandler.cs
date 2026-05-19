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
    public class DeleteInventoryFieldCommandHandler : ICQRS.IDeleteInventoryField
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public DeleteInventoryFieldCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> DeleteInventoryField(DeleteInventoryFieldCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var canManage = await _inventoryAuthorizationService.CanManageInventoryFieldsAsync(command.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<Guid>.Forbidden("Only the inventory owner or an admin can delete inventory fields.");
            }

            var field = await _context.InventoryFields
                .FirstOrDefaultAsync(x => x.Id == command.FieldId && x.InventoryId == command.InventoryId && !x.IsDeleted, cancellationToken);

            if (field is null)
            {
                return Result<Guid>.NotFound("Inventory field not found.");
            }

            var existingValues = await _context.ItemFieldValues
                .Where(x => x.InventoryFieldId == command.FieldId)
                .ToListAsync(cancellationToken);

            if (existingValues.Count > 0)
            {
                _context.ItemFieldValues.RemoveRange(existingValues);
            }

            var utcNow = DateTime.UtcNow;
            field.IsDeleted = true;
            field.DeletedAtUtc = utcNow;
            field.UpdatedAtUtc = utcNow;
            field.UpdatedByUserId = userResult.Value.Id;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(command.InventoryId);
        }
    }
}
