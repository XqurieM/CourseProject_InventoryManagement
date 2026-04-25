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
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return Result<Guid>.Invalid(new ValidationError
                {
                    Identifier = nameof(command.Name),
                    ErrorMessage = "InventoryField name is required."
                });
            }

            var existInventory = await _context.Inventories.AnyAsync(x => x.Id == command.InventoryId && x.IsDeleted == false, cancellationToken);
            if (!existInventory)
            {
                return Result<Guid>.NotFound();
            }
            await _context.InventoryFields.AddAsync(new InventoryField
            {
                InventoryId = command.InventoryId,
                Name = command.Name,
                Description = command.Description,
                FieldType = command.FieldType,
                DisplayOrder = command.DisplayOrder,
                IsRequired = command.IsRequired,
                ShowInTable = command.ShowInTable,
                CreatedByUserId = command.CreatedByUserId
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(Guid.NewGuid());
        }

    }

}
