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

            var accessList = await (
                from access in _context.InventoryAccesses
                join user in _context.Users on access.UserId equals user.Id
                where access.InventoryId == inventoryId
                orderby access.CreatedAtUtc
                select new InventoryAccessListDto
                {
                    Id = access.Id,
                    InventoryId = access.InventoryId,
                    UserId = access.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin,
                    IsBlocked = user.IsBlocked,
                    CreatedAtUtc = access.CreatedAtUtc,
                    CreatedByUserId = access.CreatedByUserId,
                    UpdatedAtUtc = access.UpdatedAtUtc ?? access.CreatedAtUtc,
                    UpdatedByUserId = access.UpdatedByUserId ?? access.CreatedByUserId
                })
                .ToListAsync(cancellationToken);

            return Result<List<InventoryAccessListDto>>.Success(accessList);
        }
    }
}
