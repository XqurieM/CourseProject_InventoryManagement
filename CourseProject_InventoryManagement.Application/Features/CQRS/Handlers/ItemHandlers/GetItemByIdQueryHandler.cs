using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;

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

            var item = (from p in _context.Items
                        join j in _context.Inventories on p.InventoryId equals j.Id
                        where p.Id == itemId && !p.IsDeleted && !j.IsDeleted
                        select new ItemDto
                        {
                            Id = p.Id,
                            InventoryId = j.Id,
                            CustomId = p.CustomId,
                            RowVersion = p.RowVersion,
                            CreateAtUtc = p.CreatedAtUtc,
                            CreatedByUserId = p.CreatedByUserId,
                            UpdatedAtUtc = p.UpdatedAtUtc ?? DateTime.MinValue,
                            UpdatedByUserId = p.UpdatedByUserId ?? p.CreatedByUserId,
                            IsDeleted = p.IsDeleted,
                            DeletedAtUtc = p.DeletedAtUtc ?? DateTime.MinValue,
                            ItemName = p.ItemName,
                            LikeCount = p.Likes.Count(),
                            IsLikedByCurrentUser = p.Likes.Any(l => l.CreatedByUserId == userResult.Value.Id)
                        }).FirstOrDefault();

            if (item is null)
            {
                return Result<ItemDto>.NotFound("Item not found.");
            }

            var canViewInventory = await _inventoryAuthorizationService.CanViewInventoryAsync(item.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canViewInventory)
            {
                return Result<ItemDto>.Forbidden("You do not have permission to view this item.");
            }

            return Result.Success(item);
        }
    }
}
