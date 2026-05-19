using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class GetInventoryTagsByInventoryIdQueryHandler : ICQRS.IGetInventoryTagsByInventoryId
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetInventoryTagsByInventoryIdQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<List<TagDto>>> GetInventoryTagsByInventoryId(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<TagDto>>(userResult);
            }

            var canView = await _inventoryAuthorizationService.CanViewInventoryAsync(inventoryId, userResult.Value.Id, cancellationToken);
            if (!canView)
            {
                return Result<List<TagDto>>.Forbidden("You do not have permission to view this inventory.");
            }

            var inventoryExists = await _context.Inventories.AnyAsync(x => x.Id == inventoryId && !x.IsDeleted, cancellationToken);
            if (!inventoryExists)
            {
                return Result<List<TagDto>>.NotFound("Inventory not found.");
            }

            var tags = await _context.InventoryTags
                .Where(x => x.InventoryId == inventoryId)
                .OrderBy(x => x.Tag.Name)
                .Select(x => new TagDto
                {
                    Id = x.TagId,
                    Name = x.Tag.Name,
                    NormalizedName = x.Tag.NormalizedName,
                    InventoryCount = x.Tag.InventoryTags.Count
                })
                .ToListAsync(cancellationToken);

            return Result<List<TagDto>>.Success(tags);
        }
    }
}
