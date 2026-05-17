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
    public class GetInventoryCustomIdRulesByInventoryIdQueryHandler : ICQRS.IGetInventoryCustomIdRulesByInventoryId
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetInventoryCustomIdRulesByInventoryIdQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<List<GetInventoryCustomIdRulesByInventoryIdResult>>> GetInventoryCustomIdRulesByInventoryId(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<GetInventoryCustomIdRulesByInventoryIdResult>>(userResult);
            }

            var canManage = await _inventoryAuthorizationService.CanManageInventoryCustomIdRulesAsync(inventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<List<GetInventoryCustomIdRulesByInventoryIdResult>>.Forbidden("Only the inventory owner or an admin can view custom ID rules.");
            }

            var rules = await _context.InventoryCustomIdRules
                .AsNoTracking()
                .Where(x => x.InventoryId == inventoryId)
                .OrderBy(x => x.PartOrder)
                .Select(x => new GetInventoryCustomIdRulesByInventoryIdResult
                {
                    Id = x.Id,
                    InventoryId = x.InventoryId,
                    PartOrder = x.PartOrder,
                    PartType = x.PartType,
                    Format = x.Format,
                    StaticTextValue = x.StaticTextValue
                })
                .ToListAsync(cancellationToken);

            return Result<List<GetInventoryCustomIdRulesByInventoryIdResult>>.Success(rules);
        }
    }
}
