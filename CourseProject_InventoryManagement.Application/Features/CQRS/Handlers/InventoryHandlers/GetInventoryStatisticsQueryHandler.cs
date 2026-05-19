using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class GetInventoryStatisticsQueryHandler : ICQRS.IGetInventoryStatistics
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetInventoryStatisticsQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<GetInventoryStatisticsResult>> GetInventoryStatistics(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, GetInventoryStatisticsResult>(userResult);
            }

            var canView = await _inventoryAuthorizationService.CanViewInventoryAsync(inventoryId, userResult.Value.Id, cancellationToken);
            if (!canView)
            {
                return Result<GetInventoryStatisticsResult>.Forbidden("You do not have permission to view this inventory.");
            }

            var inventoryExists = await _context.Inventories.AnyAsync(x => x.Id == inventoryId && !x.IsDeleted, cancellationToken);
            if (!inventoryExists)
            {
                return Result<GetInventoryStatisticsResult>.NotFound("Inventory not found.");
            }

            var itemIds = await _context.Items
                .Where(x => x.InventoryId == inventoryId && !x.IsDeleted)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var result = new GetInventoryStatisticsResult
            {
                InventoryId = inventoryId,
                ItemCount = itemIds.Count,
                CommentCount = await _context.Comments.CountAsync(x => x.InventoryId == inventoryId && !x.IsDeleted, cancellationToken),
                FieldCount = await _context.InventoryFields.CountAsync(x => x.InventoryId == inventoryId && !x.IsDeleted, cancellationToken),
                TagCount = await _context.InventoryTags.CountAsync(x => x.InventoryId == inventoryId, cancellationToken),
                LikeCount = await _context.ItemLikes.CountAsync(x => itemIds.Contains(x.ItemId), cancellationToken),
                ExplicitAccessCount = await _context.InventoryAccesses.CountAsync(x => x.InventoryId == inventoryId, cancellationToken)
            };

            return Result<GetInventoryStatisticsResult>.Success(result);
        }
    }
}
