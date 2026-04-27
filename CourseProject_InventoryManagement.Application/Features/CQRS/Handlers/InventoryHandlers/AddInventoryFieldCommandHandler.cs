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
    public class AddInventoryFieldCommandHandler : ICQRS.IAddInventoryField
    {
        IAppDbContext _context;

        public AddInventoryFieldCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> AddInventoryField(AddInventoryFieldCommand command, CancellationToken cancellationToken = default)
        {
            var existingFields = await _context.InventoryFields
            .Where(x => x.InventoryId == command.InventoryId)
            .ToListAsync(cancellationToken);

            if (existingFields.Any())
            {
                _context.InventoryFields.RemoveRange(existingFields);
            }

            var newFields = command.Fields.Select(f => new InventoryField
            {
                Id = Guid.NewGuid(),
                InventoryId = command.InventoryId,
                Name = f.Name,
                Description = f.Description,
                FieldType = f.FieldType,
                DisplayOrder = f.DisplayOrder,
                IsRequired = f.IsRequired,
                ShowInTable = f.ShowInTable
            }).ToList();

            await _context.InventoryFields.AddRangeAsync(newFields, cancellationToken);

            var result = await _context.SaveChangesAsync(cancellationToken);       
                
            return Result<Guid>.Success(command.InventoryId);           
            
        }

    }

}
