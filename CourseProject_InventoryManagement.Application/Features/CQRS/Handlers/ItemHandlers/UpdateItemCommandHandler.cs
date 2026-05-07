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
    public class UpdateItemCommandHandler : ICQRS.IUpdateItem
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public UpdateItemCommandHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService, IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> UpdateItem(UpdateItemCommand command, CancellationToken cancellationToken = default)
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

            var canUpdateItem = await _inventoryAuthorizationService
                .CanUpdateItemsAsync(command.InventoryId, userResult.Value.Id, cancellationToken);

            if (!canUpdateItem)
            {
                return Result<Guid>.Forbidden("You do not have permission to update this item.");
            }

            if (command.RowVersion is null || command.RowVersion.Length == 0)
            {
                return Result<Guid>.Invalid(new ValidationError(nameof(command.RowVersion), "RowVersion is required."));
            }

            if (command.ItemName is not null)
            {
                if (string.IsNullOrWhiteSpace(command.ItemName))
                {
                    return Result<Guid>.Invalid(new ValidationError(nameof(command.ItemName), "ItemName cannot be empty."));
                }

                item.ItemName = command.ItemName.Trim();
            }

            if (command.CustomId is not null)
            {
                item.CustomId = command.CustomId.Trim();
            }          

            item.UpdatedAtUtc = DateTime.UtcNow;
            item.UpdatedByUserId = userResult.Value.Id;
            _context.Entry(item).Property(x => x.RowVersion).OriginalValue = command.RowVersion;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<Guid>.Conflict("The item was updated by another user. Please refresh and try again.");
            }

            return Result<Guid>.Success(item.Id);
        }
    }
}
