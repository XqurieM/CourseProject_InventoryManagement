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
    public class AddItemFieldValuesCommandHandler : ICQRS.IAddItemFieldValues
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public AddItemFieldValuesCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> AddItemFieldValues(AddItemFieldValuesCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var item = await _context.Items
                .Where(x => x.Id == command.ItemId)
                .Select(x => new { x.Id, x.InventoryId })
                .FirstOrDefaultAsync(cancellationToken);

            if (item is null)
            {
                return Result<Guid>.NotFound($"The item with ID '{command.ItemId}' was not found.");
            }

            var canWriteItems = await _inventoryAuthorizationService.CanWriteItemsAsync(item.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canWriteItems)
            {
                return Result<Guid>.Forbidden("You do not have permission to edit items in this inventory.");
            }

            var existingValues = await _context.ItemFieldValues
                .Where(x => x.ItemId == command.ItemId)
                .ToListAsync(cancellationToken);

            if (existingValues.Any())
            {
                _context.ItemFieldValues.RemoveRange(existingValues);
            }

            var newValues = command.Values.Select(v => new ItemFieldValue
            {
                ItemId = command.ItemId,
                InventoryFieldId = v.InventoryFieldId,
                StringValue = v.StringValue,
                NumberValue = v.NumberValue,
                BooleanValue = v.BooleanValue,
                CreatedByUserId = userResult.Value.Id
            }).ToList();

            if (newValues.Any())
            {
                await _context.ItemFieldValues.AddRangeAsync(newValues, cancellationToken);
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return Result<Guid>.Success(command.ItemId);
            }
            catch (Exception ex)
            {
                return Result<Guid>.Error($"An error occurred while saving item field values: {ex.Message}");
            }
        }
    }
}
