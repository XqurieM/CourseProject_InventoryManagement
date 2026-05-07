using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.ItemResults;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class GetItemFieldValuesByItemIdQueryHandler : ICQRS.IGetItemFieldValuesByItemId
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetItemFieldValuesByItemIdQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<List<ItemFieldValuesResult>>> GetItemFieldValuesByItemId(Guid itemId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<ItemFieldValuesResult>>(userResult);
            }

            var inventoryId = await _context.Items
                .Where(x => x.Id == itemId && !x.IsDeleted)
                .Select(x => x.InventoryId)
                .FirstOrDefaultAsync(cancellationToken);

            if (inventoryId == Guid.Empty)
            {
                return Result<List<ItemFieldValuesResult>>.NotFound("Item not found.");
            }

            var canViewInventory = await _inventoryAuthorizationService.CanViewInventoryAsync(inventoryId, userResult.Value.Id, cancellationToken);
            if (!canViewInventory)
            {
                return Result<List<ItemFieldValuesResult>>.Forbidden("You do not have permission to view field values for this item.");
            }

            var result = await _context.ItemFieldValues
                .Where(x => x.ItemId == itemId)
                .Select(x => new ItemFieldValuesResult
                {
                    Id = x.Id,
                    ItemId = x.ItemId,
                    InventoryFieldId = x.InventoryFieldId,
                    StringValue = x.StringValue ?? string.Empty,
                    NumberValue = x.NumberValue ?? 0,
                    BooleanValue = x.BooleanValue ?? false,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId,
                    UpdatedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
                    UpdatedByUserId = x.UpdatedByUserId ?? x.CreatedByUserId
                })
                .ToListAsync(cancellationToken);

            return Result<List<ItemFieldValuesResult>>.Success(result);
        }
    }
}
