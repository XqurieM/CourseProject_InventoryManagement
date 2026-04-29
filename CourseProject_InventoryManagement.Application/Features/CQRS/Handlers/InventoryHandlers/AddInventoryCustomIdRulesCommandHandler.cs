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
    public class AddInventoryCustomIdRulesCommandHandler : ICQRS.IAddInventoryCustomIdRules
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public AddInventoryCustomIdRulesCommandHandler(
            IAppDbContext appDbContext,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = appDbContext;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> AddInventoryCustomIdRules(AddInventoryCustomIdRulesCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var canManage = await _inventoryAuthorizationService.CanManageInventoryAsync(command.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<Guid>.Forbidden("Only the inventory owner or an admin can edit custom ID rules.");
            }

            var inventoryExists = await _context.Inventories.AnyAsync(x => x.Id == command.InventoryId && !x.IsDeleted, cancellationToken);
            if (!inventoryExists)
            {
                return Result<Guid>.NotFound($"Inventory with ID {command.InventoryId} not found.");
            }

            var existingRules = await _context.InventoryCustomIdRules
                .Where(x => x.InventoryId == command.InventoryId)
                .ToListAsync(cancellationToken);

            if (existingRules.Any())
            {
                _context.InventoryCustomIdRules.RemoveRange(existingRules);
            }

            var newRules = command.Rules.Select(r => new InventoryCustomIdRule
            {
                Id = Guid.NewGuid(),
                InventoryId = command.InventoryId,
                PartOrder = r.PartOrder,
                PartType = r.PartType,
                StaticTextValue = r.StaticTextValue,
                Format = r.Format,
                CreatedByUserId = userResult.Value.Id
            }).ToList();

            await _context.InventoryCustomIdRules.AddRangeAsync(newRules, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(command.InventoryId);
        }
    }
}
