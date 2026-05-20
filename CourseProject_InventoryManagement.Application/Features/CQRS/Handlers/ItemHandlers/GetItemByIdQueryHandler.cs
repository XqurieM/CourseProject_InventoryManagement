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
    public class GetItemByIdQueryHandler : ICQRS.IGetItemById
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetItemByIdQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<ItemDto>> GetItemById(Guid itemId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, ItemDto>(userResult);
            }

            var itemEntity = await _context.Items
                .Include(x => x.Inventory)
                .Include(x => x.Likes)
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDeleted, cancellationToken);

            if (itemEntity is null || itemEntity.Inventory.IsDeleted)
            {
                return Result<ItemDto>.NotFound("Item not found.");
            }

            var canViewInventory = await _inventoryAuthorizationService.CanViewInventoryAsync(itemEntity.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canViewInventory)
            {
                return Result<ItemDto>.Forbidden("You do not have permission to view this item.");
            }

            var orderedImages = itemEntity.Images
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.DisplayOrder)
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
                .ToList();

            var item = new ItemDto
            {
                Id = itemEntity.Id,
                InventoryId = itemEntity.InventoryId,
                CustomId = itemEntity.CustomId,
                RowVersion = itemEntity.RowVersion,
                CreateAtUtc = itemEntity.CreatedAtUtc,
                CreatedByUserId = itemEntity.CreatedByUserId,
                UpdatedAtUtc = itemEntity.UpdatedAtUtc ?? DateTime.MinValue,
                UpdatedByUserId = itemEntity.UpdatedByUserId ?? itemEntity.CreatedByUserId,
                IsDeleted = itemEntity.IsDeleted,
                DeletedAtUtc = itemEntity.DeletedAtUtc ?? DateTime.MinValue,
                ItemName = itemEntity.ItemName,
                LikeCount = itemEntity.Likes.Count,
                IsLikedByCurrentUser = itemEntity.Likes.Any(l => l.CreatedByUserId == userResult.Value.Id),
                PrimaryImageUrl = orderedImages.FirstOrDefault()?.ImageUrl,
                Images = orderedImages
            };

            return Result.Success(item);
        }
    }
}
