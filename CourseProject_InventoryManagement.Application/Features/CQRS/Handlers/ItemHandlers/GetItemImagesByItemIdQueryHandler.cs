using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class GetItemImagesByItemIdQueryHandler : ICQRS.IGetItemImagesByItemId
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetItemImagesByItemIdQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<List<ItemImageDto>>> GetItemImagesByItemId(Guid itemId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<ItemImageDto>>(userResult);
            }

            var item = await _context.Items
                .Where(x => x.Id == itemId && !x.IsDeleted)
                .Select(x => new { x.Id, x.InventoryId })
                .FirstOrDefaultAsync(cancellationToken);

            if (item is null)
            {
                return Result<List<ItemImageDto>>.NotFound("Item not found.");
            }

            var canView = await _inventoryAuthorizationService.CanViewInventoryAsync(item.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canView)
            {
                return Result<List<ItemImageDto>>.Forbidden("You do not have permission to view this item.");
            }

            var images = await _context.ItemImages
                .Where(x => x.ItemId == itemId)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.CreatedAtUtc)
                .Select(x => new ItemImageDto
                {
                    Id = x.Id,
                    ItemId = x.ItemId,
                    ImageUrl = x.ImageUrl,
                    Caption = x.Caption,
                    DisplayOrder = x.DisplayOrder,
                    IsPrimary = x.IsPrimary
                })
                .ToListAsync(cancellationToken);

            return Result.Success(images);
        }
    }
}
