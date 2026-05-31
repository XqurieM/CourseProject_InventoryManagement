using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class DeleteOdooItemCommandHandler : ICQRS.IDeleteOdooItem
    {
        private readonly IAppDbContext _context;

        public DeleteOdooItemCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result> DeleteOdooItem(DeleteOdooItemCommand command, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command.Token))
            {
                return Result.Invalid(new ValidationError { ErrorMessage = "Token is required." });
            }

            if (string.IsNullOrWhiteSpace(command.CustomId))
            {
                return Result.Invalid(new ValidationError { ErrorMessage = "Custom ID is required." });
            }

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(x => x.ApiToken == command.Token && !x.IsDeleted, cancellationToken);

            if (inventory is null)
            {
                return Result.NotFound("No inventory found matching the provided integration token.");
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(x => x.InventoryId == inventory.Id && x.CustomId == command.CustomId && !x.IsDeleted, cancellationToken);

            if (item is null)
            {
                return Result.NotFound("Item not found.");
            }

            var now = DateTime.UtcNow;
            item.IsDeleted = true;
            item.DeletedAtUtc = now;
            item.UpdatedAtUtc = now;
            item.UpdatedByUserId = inventory.CreatedByUserId;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
