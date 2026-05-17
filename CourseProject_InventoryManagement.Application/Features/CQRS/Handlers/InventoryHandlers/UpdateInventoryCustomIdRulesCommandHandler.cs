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
    public class UpdateInventoryCustomIdRulesCommandHandler : ICQRS.IUpdateInventoryCustomIdRules
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public UpdateInventoryCustomIdRulesCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> UpdateInventoryCustomIdRules(UpdateInventoryCustomIdRulesCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var canManage = await _inventoryAuthorizationService.CanManageInventoryCustomIdRulesAsync(command.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<Guid>.Forbidden("Only the inventory owner or an admin can edit custom ID rules.");
            }

            var inventoryExists = await _context.Inventories.AnyAsync(x => x.Id == command.InventoryId && !x.IsDeleted, cancellationToken);
            if (!inventoryExists)
            {
                return Result<Guid>.NotFound($"Inventory with ID {command.InventoryId} not found.");
            }

            var ruleIds = command.Rules.Select(x => x.Id).Distinct().ToList();
            var existingRules = await _context.InventoryCustomIdRules
                .Where(x => x.InventoryId == command.InventoryId && ruleIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            if (existingRules.Count != ruleIds.Count)
            {
                return Result<Guid>.NotFound("One or more custom ID rules could not be found.");
            }

            var utcNow = DateTime.UtcNow;
            foreach (var existingRule in existingRules)
            {
                var input = command.Rules.First(x => x.Id == existingRule.Id);
                existingRule.PartOrder = input.PartOrder;
                existingRule.PartType = input.PartType;
                existingRule.Format = string.IsNullOrWhiteSpace(input.Format) ? null : input.Format.Trim();
                existingRule.StaticTextValue = string.IsNullOrWhiteSpace(input.StaticTextValue) ? null : input.StaticTextValue.Trim();
                existingRule.UpdatedAtUtc = utcNow;
                existingRule.UpdatedByUserId = userResult.Value.Id;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(command.InventoryId);
        }
    }
}
