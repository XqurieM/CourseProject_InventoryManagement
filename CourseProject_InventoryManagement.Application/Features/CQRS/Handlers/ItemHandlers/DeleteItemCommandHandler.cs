using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class DeleteItemCommandHandler : ICQRS.IDeleteItem
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public DeleteItemCommandHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService, IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> DeleteItem(DeleteItemCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken);

            if (item is null)
            {
                return Result<Guid>.NotFound("Item not found.");
            }

            var canManage = await _inventoryAuthorizationService
                .CanManageInventoryAsync(item.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<Guid>.Forbidden("You do not have permission to delete this item.");
            }

            var utcNow = DateTime.UtcNow;

            item.IsDeleted = true;
            item.DeletedAtUtc = utcNow;
            item.UpdatedAtUtc = utcNow;
            item.UpdatedByUserId = userResult.Value.Id;            

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(item.Id);
        }
    }
}
