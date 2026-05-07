using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CategoryCommands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.CategoryHandlers
{
    public class DeleteCategoryCommandHandler : ICQRS.IDeleteCategory
    {
        private readonly IAppDbContext _context;

        public DeleteCategoryCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> DeleteCategory(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == command.CategoryId, cancellationToken);

            if (category is null)
            {
                return Result<Guid>.NotFound("Category not found.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(category.Id);
        }
    }
}
