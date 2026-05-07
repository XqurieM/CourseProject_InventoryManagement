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
    public class GetAllCategoriesQueryHandler : ICQRS.IGetAllCategories
    {
        private readonly IAppDbContext _context;

        public GetAllCategoriesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CategoryDto>>> GetAllCategories(CancellationToken cancellationToken = default)
        {
            var allCategories = await _context.Categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            }).ToListAsync(cancellationToken);
            return Result<List<CategoryDto>>.Success(allCategories);
        }
    }
}
