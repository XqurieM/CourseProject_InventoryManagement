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
    public class GetInventoryAccessListQueryHandler : ICQRS.IGetInventoryAccessList
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetInventoryAccessListQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<List<InventoryAccessListDto>>> GetInventoryAccessList(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<InventoryAccessListDto>>(userResult);
            }

            var canViewAccessList = await _inventoryAuthorizationService.CanViewInventoryAccessListAsync(inventoryId, userResult.Value.Id, cancellationToken);
            if (!canViewAccessList)
            {
                return Result<List<InventoryAccessListDto>>.Forbidden("You do not have permission to view access settings for this inventory.");
            }

            var accessList = await _context.InventoryAccesses
                .Where(x => x.InventoryId == inventoryId)
                .Select(x => new InventoryAccessListDto
                {
                    Id = x.Id,
                    InventoryId = x.InventoryId,
                    UserId = x.UserId,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId,
                    UpdatedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
                    UpdatedByUserId = x.UpdatedByUserId ?? x.CreatedByUserId
                })
                .ToListAsync(cancellationToken);

            return Result<List<InventoryAccessListDto>>.Success(accessList);
        }
    }
}
