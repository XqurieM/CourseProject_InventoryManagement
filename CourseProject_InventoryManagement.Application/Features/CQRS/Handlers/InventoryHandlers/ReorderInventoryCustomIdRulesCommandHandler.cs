using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class ReorderInventoryCustomIdRulesCommandHandler : ICQRS.IReorderInventoryCustomIdRules
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public ReorderInventoryCustomIdRulesCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> ReorderInventoryCustomIdRules(ReorderInventoryCustomIdRulesCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var canManage = await _inventoryAuthorizationService.CanManageInventoryCustomIdRulesAsync(command.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<Guid>.Forbidden("Only the inventory owner or an admin can reorder custom ID rules.");
            }

            var items = command.Rules
                .Where(x => x.RuleId != Guid.Empty)
                .GroupBy(x => x.RuleId)
                .Select(x => x.First())
                .ToList();

            if (items.Count == 0)
            {
                return Result<Guid>.Invalid(new ValidationError(nameof(command.Rules), "At least one rule order is required."));
            }

            var ids = items.Select(x => x.RuleId).ToList();
            var existingRules = await _context.InventoryCustomIdRules
                .Where(x => x.InventoryId == command.InventoryId && ids.Contains(x.Id))
                .ToListAsync(cancellationToken);

            if (existingRules.Count != ids.Count)
            {
                return Result<Guid>.NotFound("One or more custom ID rules could not be found.");
            }

            var utcNow = DateTime.UtcNow;
            foreach (var rule in existingRules)
            {
                var match = items.First(x => x.RuleId == rule.Id);
                rule.PartOrder = match.PartOrder;
                rule.UpdatedAtUtc = utcNow;
                rule.UpdatedByUserId = userResult.Value.Id;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(command.InventoryId);
        }
    }
}
