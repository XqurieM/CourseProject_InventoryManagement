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
    public class GetItemsByInventoryIdQueryHandler : ICQRS.IGetItemsByInventoryId
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetItemsByInventoryIdQueryHandler(
            IAppDbContext appDbContext,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = appDbContext;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<List<ItemDto>>> GetItemsByInventoryId(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<ItemDto>>(userResult);
            }

            var canViewInventory = await _inventoryAuthorizationService.CanViewInventoryAsync(inventoryId, userResult.Value.Id, cancellationToken);
            if (!canViewInventory)
            {
                return Result<List<ItemDto>>.Forbidden("You do not have permission to view items for this inventory.");
            }

            var items = from p in _context.Items
                        join j in _context.Inventories on p.InventoryId equals j.Id
                        where p.InventoryId == inventoryId && !p.IsDeleted && !j.IsDeleted
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
                            IsLikedByCurrentUser = p.Likes.Any(l => l.CreatedByUserId == userResult.Value.Id),
                            PrimaryImageUrl = p.Images
                                .OrderByDescending(img => img.IsPrimary)
                                .ThenBy(img => img.DisplayOrder)
                                .Select(img => img.ImageUrl)
                                .FirstOrDefault()
                        };

            return Result.Success(await items.ToListAsync(cancellationToken));
        }
    }
}
