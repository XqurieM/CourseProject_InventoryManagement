using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class AddInventoryCustomIdRulesHandler : ICQRS.IAddInventoryCustomIdRules
    {
        IAppDbContext _context;

        public AddInventoryCustomIdRulesHandler(IAppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task<Result<Guid>> AddInventoryCustomIdRules(AddInventoryCustomIdRulesCommand command, CancellationToken cancellationToken = default)
        {  
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
                Format = r.Format
            }).ToList();

            await _context.InventoryCustomIdRules.AddRangeAsync(newRules, cancellationToken);

            var result = await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(Guid.NewGuid());

        }
    }
}
