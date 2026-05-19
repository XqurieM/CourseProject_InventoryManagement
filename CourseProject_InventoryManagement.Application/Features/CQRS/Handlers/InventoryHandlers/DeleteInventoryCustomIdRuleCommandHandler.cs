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
    public class DeleteInventoryCustomIdRuleCommandHandler : ICQRS.IDeleteInventoryCustomIdRule
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public DeleteInventoryCustomIdRuleCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> DeleteInventoryCustomIdRule(DeleteInventoryCustomIdRuleCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var canManage = await _inventoryAuthorizationService.CanManageInventoryCustomIdRulesAsync(command.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<Guid>.Forbidden("Only the inventory owner or an admin can delete custom ID rules.");
            }

            var rule = await _context.InventoryCustomIdRules
                .FirstOrDefaultAsync(x => x.Id == command.RuleId && x.InventoryId == command.InventoryId, cancellationToken);

            if (rule is null)
            {
                return Result<Guid>.NotFound("Custom ID rule not found.");
            }

            _context.InventoryCustomIdRules.Remove(rule);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(command.InventoryId);
        }
    }
}
