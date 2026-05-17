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
    public class UpdateInventoryFieldsCommandHandler : ICQRS.IUpdateInventoryFields
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public UpdateInventoryFieldsCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> UpdateInventoryFields(UpdateInventoryFieldsCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var canManage = await _inventoryAuthorizationService.CanManageInventoryAsync(command.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<Guid>.Forbidden("Only the inventory owner or an admin can edit inventory fields.");
            }

            var inventoryExists = await _context.Inventories
                .AnyAsync(x => x.Id == command.InventoryId && !x.IsDeleted, cancellationToken);

            if (!inventoryExists)
            {
                return Result<Guid>.NotFound("Inventory not found.");
            }

            var validFields = command.Fields
                .Where(x => x.Id != Guid.Empty && !string.IsNullOrWhiteSpace(x.Name))
                .ToList();

            if (validFields.Count == 0)
            {
                return Result<Guid>.Invalid(new ValidationError
                {
                    Identifier = nameof(command.Fields),
                    ErrorMessage = "Update at least one valid field."
                });
            }

            var requestedIds = validFields.Select(x => x.Id).Distinct().ToList();
            var existingFields = await _context.InventoryFields
                .Where(x => x.InventoryId == command.InventoryId && !x.IsDeleted && requestedIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            if (existingFields.Count != requestedIds.Count)
            {
                return Result<Guid>.NotFound("One or more inventory fields could not be found.");
            }

            foreach (var field in existingFields)
            {
                var updated = validFields.First(x => x.Id == field.Id);
                field.Name = updated.Name.Trim();
                field.Description = string.IsNullOrWhiteSpace(updated.Description) ? null : updated.Description.Trim();
                field.FieldType = updated.FieldType;
                field.DisplayOrder = updated.DisplayOrder > 0 ? updated.DisplayOrder : field.DisplayOrder;
                field.IsRequired = updated.IsRequired;
                field.ShowInTable = updated.ShowInTable;
                field.UpdatedAtUtc = DateTime.UtcNow;
                field.UpdatedByUserId = userResult.Value.Id;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(command.InventoryId);
        }
    }
}
