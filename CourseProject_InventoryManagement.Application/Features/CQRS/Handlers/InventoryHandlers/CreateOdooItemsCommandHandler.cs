using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Domain.Entities;
using CourseProject_InventoryManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class CreateOdooItemsCommandHandler : ICQRS.ICreateOdooItems
    {
        private readonly IAppDbContext _context;
        private readonly ICustomIdGenerator _idGenerator;

        public CreateOdooItemsCommandHandler(IAppDbContext context, ICustomIdGenerator idGenerator)
        {
            _context = context;
            _idGenerator = idGenerator;
        }

        public async Task<Result<List<Guid>>> CreateOdooItems(CreateOdooItemsCommand command, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command.Token))
            {
                return Result<List<Guid>>.Invalid(new ValidationError { ErrorMessage = "Token cannot be empty." });
            }

            if (command.Items == null || !command.Items.Any())
            {
                return Result<List<Guid>>.Invalid(new ValidationError { ErrorMessage = "Items list cannot be empty." });
            }

            var inventory = await _context.Inventories
                .Include(x => x.CustomIdRules)
                .Include(x => x.Fields)
                .FirstOrDefaultAsync(x => x.ApiToken == command.Token && !x.IsDeleted, cancellationToken);

            if (inventory is null)
            {
                return Result<List<Guid>>.NotFound("No inventory found matching the provided integration token.");
            }

            var rules = inventory.CustomIdRules.OrderBy(x => x.PartOrder).ToList();
            var currentCount = await _context.Items.CountAsync(x => x.InventoryId == inventory.Id, cancellationToken);

            var createdIds = new List<Guid>();
            var now = DateTime.UtcNow;

            foreach (var itemDto in command.Items)
            {
                if (string.IsNullOrWhiteSpace(itemDto.ItemName))
                {
                    return Result<List<Guid>>.Invalid(new ValidationError { ErrorMessage = "Item name is required for all items." });
                }

                string finalCustomId;
                if (!string.IsNullOrWhiteSpace(itemDto.CustomId))
                {
                    finalCustomId = itemDto.CustomId.Trim();
                }
                else
                {
                    finalCustomId = _idGenerator.Generate(rules, currentCount);
                }

                currentCount++;

                var existingItem = await _context.Items
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.InventoryId == inventory.Id && x.CustomId == finalCustomId, cancellationToken);

                Guid itemId;
                if (existingItem != null)
                {
                    itemId = existingItem.Id;
                    existingItem.ItemName = itemDto.ItemName.Trim();
                    existingItem.IsDeleted = false;
                    existingItem.DeletedAtUtc = null;
                    existingItem.UpdatedAtUtc = now;
                    existingItem.UpdatedByUserId = inventory.CreatedByUserId;

                    _context.Items.Update(existingItem);

                    var oldFieldVals = await _context.ItemFieldValues
                        .Where(x => x.ItemId == itemId)
                        .ToListAsync(cancellationToken);
                    _context.ItemFieldValues.RemoveRange(oldFieldVals);
                }
                else
                {
                    var newItem = new Item
                    {
                        Id = Guid.NewGuid(),
                        ItemName = itemDto.ItemName.Trim(),
                        InventoryId = inventory.Id,
                        CustomId = finalCustomId,
                        CreatedAtUtc = now,
                        CreatedByUserId = inventory.CreatedByUserId,
                        IsDeleted = false
                    };
                    itemId = newItem.Id;
                    await _context.Items.AddAsync(newItem, cancellationToken);
                }

                createdIds.Add(itemId);

                if (itemDto.FieldValues != null)
                {
                    foreach (var valDto in itemDto.FieldValues)
                    {
                        if (string.IsNullOrWhiteSpace(valDto.FieldName)) continue;

                        var field = inventory.Fields.FirstOrDefault(f => f.Name.Equals(valDto.FieldName, StringComparison.OrdinalIgnoreCase) && !f.IsDeleted);
                        if (field != null)
                        {
                            var newVal = new ItemFieldValue
                            {
                                Id = Guid.NewGuid(),
                                ItemId = itemId,
                                InventoryFieldId = field.Id,
                                CreatedAtUtc = now,
                                CreatedByUserId = inventory.CreatedByUserId
                            };

                            if (field.FieldType == Domain.Enums.InventoryFieldType.Number)
                            {
                                if (decimal.TryParse(valDto.Value, out decimal parsedNum))
                                {
                                    newVal.NumberValue = parsedNum;
                                }
                            }
                            else if (field.FieldType == Domain.Enums.InventoryFieldType.Boolean)
                            {
                                if (bool.TryParse(valDto.Value, out bool parsedBool))
                                {
                                    newVal.BooleanValue = parsedBool;
                                }
                                else if (valDto.Value == "1" || valDto.Value == "0")
                                {
                                    newVal.BooleanValue = valDto.Value == "1";
                                }
                            }
                            else
                            {
                                newVal.StringValue = valDto.Value;
                            }

                            await _context.ItemFieldValues.AddAsync(newVal, cancellationToken);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<Guid>>.Success(createdIds);
        }
    }
}
