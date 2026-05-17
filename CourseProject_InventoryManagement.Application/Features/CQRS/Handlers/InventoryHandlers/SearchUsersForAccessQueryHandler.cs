using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class SearchUsersForAccessQueryHandler : ICQRS.ISearchUsersForAccess
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public SearchUsersForAccessQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<List<InventoryAccessUserLookupDto>>> SearchUsersForAccess(SearchUsersForAccessQuery query, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<InventoryAccessUserLookupDto>>(userResult);
            }

            var canManage = await _inventoryAuthorizationService.CanManageInventoryAccessAsync(query.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<List<InventoryAccessUserLookupDto>>.Forbidden("Only the inventory owner or an admin can search users for access.");
            }

            var searchTerm = query.SearchTerm?.Trim();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Result<List<InventoryAccessUserLookupDto>>.Success([]);
            }

            var normalized = searchTerm.ToLower();

            var existingUserIds = await _context.InventoryAccesses
                .Where(x => x.InventoryId == query.InventoryId)
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);

            var users = await _context.Users
                .Where(x =>
                    !x.IsDeleted &&
                    !x.IsBlocked &&
                    x.Id != userResult.Value.Id &&
                    (x.UserName.ToLower().Contains(normalized) || x.Email.ToLower().Contains(normalized)))
                .OrderBy(x => x.UserName)
                .ThenBy(x => x.Email)
                .Take(10)
                .Select(x => new InventoryAccessUserLookupDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    IsAdmin = x.IsAdmin,
                    IsAlreadyAdded = existingUserIds.Contains(x.Id)
                })
                .ToListAsync(cancellationToken);

            return Result<List<InventoryAccessUserLookupDto>>.Success(users);
        }
    }
}
