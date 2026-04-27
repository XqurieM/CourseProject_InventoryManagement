using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using CourseProject_InventoryManagement.Domain.Entities;
using CourseProject_InventoryManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class AddItemCommandHandler : ICQRS.IAddItem
    {
        IAppDbContext _context;
        private readonly ICustomIdGenerator _idGenerator;

        public AddItemCommandHandler(IAppDbContext context, ICustomIdGenerator idGenerator)
        {
            _context = context;
            _idGenerator = idGenerator;
        }

        public async Task<Result<List<Guid>>> AddItem(AddItemCommand command, CancellationToken cancellationToken = default)
        {
            var inventoryExists = await _context.Inventories
            .AnyAsync(x => x.Id == command.InventoryId, cancellationToken);

            if (!inventoryExists)
            {
                return Result.NotFound($"The inventory with ID '{command.InventoryId}' was not found.");
            }

            if (command.Items == null || !command.Items.Any())
            {
                return Result.Invalid(new ValidationError("The item list cannot be empty."));
            }

            var rules = await _context.InventoryCustomIdRules
                .Where(x => x.InventoryId == command.InventoryId)
                .OrderBy(x => x.PartOrder)
                .ToListAsync(cancellationToken);

            var currentCount = await _context.Items
                .CountAsync(x => x.InventoryId == command.InventoryId, cancellationToken);

            var newItems = new List<Item>();
            var now = DateTime.UtcNow;

            try
            {
                foreach (var itemDto in command.Items)
                {
                    string generatedCustomId = _idGenerator.Generate(rules, currentCount);
                    currentCount++;

                    var newItem = new Item
                    {
                        Id = Guid.NewGuid(),
                        ItemName = itemDto.ItemName,
                        InventoryId = command.InventoryId,
                        CustomId = generatedCustomId,
                        CreatedAtUtc = now,
                        CreatedByUserId = command.CreatedByUserId,
                        IsDeleted = false
                    };

                    newItems.Add(newItem);
                }

                await _context.Items.AddRangeAsync(newItems, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success(newItems.Select(x => x.Id).ToList());
            }
            catch (Exception ex)
            {
                return Result.Error($"An error occurred during bulk item insertion: {ex.Message}");
            }
        }
    }
}
