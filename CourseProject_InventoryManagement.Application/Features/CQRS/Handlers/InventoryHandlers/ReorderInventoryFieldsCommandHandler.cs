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
    public class ReorderInventoryFieldsCommandHandler : ICQRS.IReorderInventoryFields
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public ReorderInventoryFieldsCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> ReorderInventoryFields(ReorderInventoryFieldsCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var canManage = await _inventoryAuthorizationService.CanManageInventoryFieldsAsync(command.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<Guid>.Forbidden("Only the inventory owner or an admin can reorder inventory fields.");
            }

            var items = command.Fields
                .Where(x => x.FieldId != Guid.Empty)
                .GroupBy(x => x.FieldId)
                .Select(x => x.First())
                .ToList();

            if (items.Count == 0)
            {
                return Result<Guid>.Invalid(new ValidationError(nameof(command.Fields), "At least one field order is required."));
            }

            var ids = items.Select(x => x.FieldId).ToList();
            var existingFields = await _context.InventoryFields
                .Where(x => x.InventoryId == command.InventoryId && !x.IsDeleted && ids.Contains(x.Id))
                .ToListAsync(cancellationToken);

            if (existingFields.Count != ids.Count)
            {
                return Result<Guid>.NotFound("One or more inventory fields could not be found.");
            }

            var utcNow = DateTime.UtcNow;
            foreach (var field in existingFields)
            {
                var match = items.First(x => x.FieldId == field.Id);
                field.DisplayOrder = match.DisplayOrder;
                field.UpdatedAtUtc = utcNow;
                field.UpdatedByUserId = userResult.Value.Id;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(command.InventoryId);
        }
    }
}
