using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.CategoryHandlers
{
    public class GetCategoryByIdQueryHandler : ICQRS.IGetCategoryById
    {
        private readonly IAppDbContext _context;

        public GetCategoryByIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CategoryDto>> GetCategoryById(Guid categoryId, CancellationToken cancellationToken = default)
        {
            var category = await _context.Categories.Where(c => c.Id == categoryId).Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            }).FirstOrDefaultAsync(cancellationToken);

            if (category == null)
            {
                return Result<CategoryDto>.NotFound();
            }

            return Result<CategoryDto>.Success(category);
        }
    }
}
