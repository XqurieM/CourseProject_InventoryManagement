using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using CourseProject_InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class AddItemCommandHandler : ICQRS.IAddItem
    {
        IAppDbContext _context;

        public AddItemCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> AddItem(AddItemCommand command, CancellationToken cancellationToken = default)
        {
            var existingInventory = await _context.Inventories.FindAsync(new object[] { command.InventoryId }, cancellationToken);
            if (existingInventory == null)
            {
                return Result<Guid>.NotFound();
            }

            var newItem = new Item
            {
                InventoryId = command.InventoryId,
                CustomId = command.CustomId,
                CreatedByUserId = command.CreatedByUserId
            };

            _context.Items.Add(newItem);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(newItem.Id);
        }
    }
}
