using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class UpdateItemFieldValuesCommandHandler : ICQRS.IUpdateItemFieldValues
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public UpdateItemFieldValuesCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> UpdateItemFieldValues(UpdateItemFieldValuesCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var item = await _context.Items
                .Where(x => x.Id == command.ItemId && !x.IsDeleted)
                .Select(x => new { x.Id, x.InventoryId })
                .FirstOrDefaultAsync(cancellationToken);

            if (item is null)
            {
                return Result<Guid>.NotFound("Item not found.");
            }

            var canUpdate = await _inventoryAuthorizationService.CanUpdateItemsAsync(item.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canUpdate)
            {
                return Result<Guid>.Forbidden("You do not have permission to update item field values in this inventory.");
            }

            var existingValues = await _context.ItemFieldValues
                .Where(x => x.ItemId == command.ItemId)
                .ToListAsync(cancellationToken);

            if (existingValues.Count > 0)
            {
                _context.ItemFieldValues.RemoveRange(existingValues);
            }

            var values = command.Values.Select(x => new ItemFieldValue
            {
                ItemId = command.ItemId,
                InventoryFieldId = x.InventoryFieldId,
                StringValue = string.IsNullOrWhiteSpace(x.StringValue) ? null : x.StringValue.Trim(),
                NumberValue = x.NumberValue,
                BooleanValue = x.BooleanValue,
                CreatedByUserId = userResult.Value.Id
            }).ToList();

            if (values.Count > 0)
            {
                await _context.ItemFieldValues.AddRangeAsync(values, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(command.ItemId);
        }
    }
}
