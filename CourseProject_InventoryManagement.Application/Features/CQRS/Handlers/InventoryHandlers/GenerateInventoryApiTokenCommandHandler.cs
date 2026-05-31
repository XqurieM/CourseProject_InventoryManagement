using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class GenerateInventoryApiTokenCommandHandler : ICQRS.IGenerateInventoryApiToken
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GenerateInventoryApiTokenCommandHandler(
            IAppDbContext context,
            ICurrentUserService currentUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<string>> GenerateInventoryApiToken(GenerateInventoryApiTokenCommand command, CancellationToken cancellationToken = default)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Result<string>.Unauthorized();
            }

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(x => x.Id == command.InventoryId && !x.IsDeleted, cancellationToken);

            if (inventory is null)
            {
                return Result<string>.NotFound("Inventory not found.");
            }

            // Verify permission
            var canManage = await _inventoryAuthorizationService.CanManageInventoryAsync(command.InventoryId, _currentUserService.UserId.Value, cancellationToken);
            if (!canManage)
            {
                return Result<string>.Forbidden("You do not have permission to manage this inventory.");
            }

            // Generate a secure unique token
            string token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            inventory.ApiToken = token;

            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(token);
        }
    }
}
