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
    public class GetMyEditableInventoriesQueryHandler : ICQRS.IGetMyEditableInventories
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IUserAuthorizationService _userAuthorizationService;

        public GetMyEditableInventoriesQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IUserAuthorizationService userAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _userAuthorizationService = userAuthorizationService;
        }

        public async Task<Result<List<GetProfileInventoriesResult>>> GetMyEditableInventories(Guid UserId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<GetProfileInventoriesResult>>(userResult);
            }

            var canAccess = await _userAuthorizationService.CanAccessUserScopedDataAsync(UserId, userResult.Value.Id, cancellationToken);
            if (!canAccess)
            {
                return Result<List<GetProfileInventoriesResult>>.Forbidden("You do not have permission to view these inventories.");
            }

            var result = from p in _context.Inventories
                         join u in _context.Users on p.CreatedByUserId equals u.Id
                         join i in _context.InventoryAccesses on p.Id equals i.InventoryId
                         where i.UserId == UserId && !p.IsDeleted
                         select new GetProfileInventoriesResult
                         {
                             Id = p.Id,
                             Title = p.Title,
                             ImageUrl = p.ImageUrl,
                             CategoryId = p.CategoryId,
                             CategoryName = p.Category.Name,
                             Description = p.Description,
                             IsPublic = p.IsPublic,
                             ItemCount = _context.Items.Count(x => x.InventoryId == p.Id && !x.IsDeleted),
                             UserName = u.UserName,
                             CreatedByUserId = u.Id,
                             UpdateDate = p.UpdatedAtUtc ?? p.CreatedAtUtc
                         };

            return Result.Success(await result.ToListAsync(cancellationToken));
        }
    }
}
