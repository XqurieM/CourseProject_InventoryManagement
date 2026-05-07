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
    public class GetInventoryFieldsByInventoryIdQueryHandler : ICQRS.IGetInventoryFieldsByInventoryId
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetInventoryFieldsByInventoryIdQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<List<GetInventoryFieldsByInventoryIdResult>>> GetInventoryFieldsByInventoryId(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<GetInventoryFieldsByInventoryIdResult>>(userResult);
            }

            var canViewInventory = await _inventoryAuthorizationService.CanViewInventoryAsync(inventoryId, userResult.Value.Id, cancellationToken);
            if (!canViewInventory)
            {
                return Result<List<GetInventoryFieldsByInventoryIdResult>>.Forbidden("You do not have permission to view fields for this inventory.");
            }

            var result = await _context.InventoryFields
                .Where(x => x.InventoryId == inventoryId && !x.IsDeleted)
                .Select(x => new GetInventoryFieldsByInventoryIdResult
                {
                    Id = x.Id,
                    InventoryId = x.InventoryId,
                    Name = x.Name,
                    Description = x.Description,
                    FieldType = x.FieldType,
                    DisplayOrder = x.DisplayOrder,
                    IsRequired = x.IsRequired,
                    ShowInTable = x.ShowInTable,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId,
                    UpdatedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
                    UpdatedByUserId = x.UpdatedByUserId ?? x.CreatedByUserId,
                    IsDeleted = x.IsDeleted,
                    DeletedAtUtc = x.DeletedAtUtc ?? x.CreatedAtUtc,
                })
                .ToListAsync(cancellationToken);

            return Result<List<GetInventoryFieldsByInventoryIdResult>>.Success(result);
        }
    }
}
