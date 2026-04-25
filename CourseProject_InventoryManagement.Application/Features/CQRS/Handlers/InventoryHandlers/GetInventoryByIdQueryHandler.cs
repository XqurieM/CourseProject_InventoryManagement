using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class GetInventoryByIdQueryHandler : ICQRS.IGetInventoryById
    {
        IAppDbContext _context;

        public GetInventoryByIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<InventoryDto>> GetInventoryById(GetInventoryByIdQuery query, CancellationToken cancellationToken = default)
        {
            var inventory = await _context.Inventories
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new InventoryDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                CategoryName = x.Category.Name,
                IsPublic = x.IsPublic,
                ImageUrl = x.ImageUrl,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

            if (inventory is null)
            {
                return Result<InventoryDto>.NotFound("Inventory not found.");
            }

            return Result<InventoryDto>.Success(inventory);
        }
    }
}
