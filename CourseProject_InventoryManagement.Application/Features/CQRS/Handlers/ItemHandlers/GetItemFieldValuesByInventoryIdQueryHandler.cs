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
    public class GetItemFieldValuesByInventoryIdQueryHandler : ICQRS.IGetItemFieldValuesByInventoryId
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetItemFieldValuesByInventoryIdQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<List<ItemFieldValuesResult>>> GetItemFieldValuesByInventoryId(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<ItemFieldValuesResult>>(userResult);
            }

            var canViewInventory = await _inventoryAuthorizationService.CanViewInventoryAsync(inventoryId, userResult.Value.Id, cancellationToken);
            if (!canViewInventory)
            {
                return Result<List<ItemFieldValuesResult>>.Forbidden("You do not have permission to view field values for this inventory.");
            }

            var result = await (from p in _context.ItemFieldValues
                                join i in _context.Items on p.ItemId equals i.Id
                                where i.InventoryId == inventoryId && !i.IsDeleted
                                select new ItemFieldValuesResult
                                {
                                    Id = p.Id,
                                    ItemId = p.ItemId,
                                    InventoryFieldId = p.InventoryFieldId,
                                    StringValue = p.StringValue ?? string.Empty,
                                    NumberValue = p.NumberValue ?? 0,
                                    BooleanValue = p.BooleanValue ?? false,
                                    CreatedAtUtc = p.CreatedAtUtc,
                                    CreatedByUserId = p.CreatedByUserId,
                                    UpdatedAtUtc = p.UpdatedAtUtc ?? p.CreatedAtUtc,
                                    UpdatedByUserId = p.UpdatedByUserId ?? p.CreatedByUserId
                                }).ToListAsync(cancellationToken);

            return Result<List<ItemFieldValuesResult>>.Success(result);
        }
    }
}
