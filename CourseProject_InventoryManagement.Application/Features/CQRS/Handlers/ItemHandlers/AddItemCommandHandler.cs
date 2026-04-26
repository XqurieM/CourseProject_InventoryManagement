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

        public async Task<Result<Guid>> AddItem(AddItemCommand command, CancellationToken cancellationToken = default)
        {
            var rules = await _context.InventoryCustomIdRules
                .Where(x => x.InventoryId == command.InventoryId)
                .OrderBy(x => x.PartOrder)
                .ToListAsync(cancellationToken);

            var currentCount = await _context.Items
                .CountAsync(x => x.InventoryId == command.InventoryId, cancellationToken);

            string generatedCustomId = _idGenerator.Generate(rules, currentCount);

            var newItem = new Item
            {
                Id = Guid.NewGuid(),
                InventoryId = command.InventoryId,
                CustomId = generatedCustomId,
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Items.Add(newItem);
            await _context.SaveChangesAsync(cancellationToken);

            return newItem.Id; 
        }
    }
}
