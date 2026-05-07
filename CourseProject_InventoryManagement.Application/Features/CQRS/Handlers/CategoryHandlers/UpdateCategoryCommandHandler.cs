using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CategoryCommands;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.CategoryHandlers
{
    public class UpdateCategoryCommandHandler : ICQRS.IUpdateCategory
    {
        private readonly IAppDbContext _context;

        public UpdateCategoryCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> UpdateCategory(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == command.CategoryId, cancellationToken);

            if (category is null)
            {
                return Result<Guid>.NotFound("Category not found.");
            }

            if (command.Name is not null)
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    return Result<Guid>.Invalid(new ValidationError(nameof(command.Name), "Name cannot be empty."));
                }

                category.Name = command.Name.Trim();
            }

            if (command.Description is not null)
            {
                category.Description = command.Description.Trim();
            }           

            if (command.IsActive.HasValue)
            {
                category.IsActive = command.IsActive.Value;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(category.Id);
        }
    }
}
