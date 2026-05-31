using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class GetInventoryByIdQueryHandler : ICQRS.IGetInventoryById
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetInventoryByIdQueryHandler(
            IAppDbContext context,
            ICurrentUserService currentUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<InventoryDto>> GetInventoryById(GetInventoryByIdQuery query, CancellationToken cancellationToken = default)
        {
            var canViewInventory = await _inventoryAuthorizationService.CanViewInventoryAsync(query.Id, _currentUserService.UserId, cancellationToken);
            if (!canViewInventory)
            {
                return Result<InventoryDto>.Forbidden("You do not have permission to view this inventory.");
            }

            var inventory = await _context.Inventories
                .AsNoTracking()
                .Where(x => x.Id == query.Id && !x.IsDeleted)
                .Select(x => new InventoryDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    IsPublic = x.IsPublic,
                    ImageUrl = x.ImageUrl,
                    RowVersion = x.RowVersion,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId,
                    ApiToken = x.ApiToken
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (inventory is null)
            {
                return Result<InventoryDto>.NotFound("Inventory not found.");
            }

            if (_currentUserService.UserId.HasValue)
            {
                inventory.CanManageInventory = await _inventoryAuthorizationService.CanManageInventoryAsync(query.Id, _currentUserService.UserId.Value, cancellationToken);
                inventory.CanWriteItems = await _inventoryAuthorizationService.CanWriteItemsAsync(query.Id, _currentUserService.UserId.Value, cancellationToken);
            }

            return Result<InventoryDto>.Success(inventory);
        }
    }
}
