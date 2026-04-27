using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class AddItemFieldValuesCommandHandler : ICQRS.IAddItemFieldValues
    {
        IAppDbContext _context;

        public AddItemFieldValuesCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> AddItemFieldValues(AddItemFieldValuesCommand command, CancellationToken cancellationToken = default)
        {
            var itemExists = await _context.Items
            .AnyAsync(x => x.Id == command.ItemId, cancellationToken);

            if (!itemExists)
            {
                return Result.NotFound($"The item with ID '{command.ItemId}' was not found.");
            }

            var existingValues = await _context.ItemFieldValues
                .Where(x => x.ItemId == command.ItemId)
                .ToListAsync(cancellationToken);

            if (existingValues.Any())
            {
                _context.ItemFieldValues.RemoveRange(existingValues);
            }

            var newValues = command.Values.Select(v => new ItemFieldValue
            {
                ItemId = command.ItemId,
                InventoryFieldId = v.InventoryFieldId,
                StringValue = v.StringValue,
                NumberValue = v.NumberValue,
                BooleanValue = v.BooleanValue
            }).ToList();

            if (newValues.Any())
            {
                await _context.ItemFieldValues.AddRangeAsync(newValues, cancellationToken);
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success(command.ItemId);
            }
            catch (Exception ex)
            {
                return Result.Error($"An error occurred while saving item field values: {ex.Message}");
            }
        }
    }
}
