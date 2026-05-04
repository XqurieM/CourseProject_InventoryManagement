using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class GetPopular5InventoriesQueryHandler : ICQRS.IGetPopular5Inventories
    {
        IAppDbContext _context;

        public GetPopular5InventoriesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<GetInventoriesWithJoinInfosResult>>> GetPopular5Inventories(CancellationToken cancellationToken = default)
        {
            var inventories = from p in _context.Inventories
                              join u in _context.Users on p.CreatedByUserId equals u.Id
                              join c in _context.Categories on p.CategoryId equals c.Id
                              select new GetInventoriesWithJoinInfosResult
                              {
                                  Id = p.Id,
                                  Title = p.Title,
                                  Description = p.Description,
                                  ImageUrl = p.ImageUrl,
                                  CategoryId = p.CategoryId,
                                  CategoryName = c.Name,
                                  CreatedByUserId = p.CreatedByUserId,
                                  UserName = u.UserName,
                                  IsPublic = p.IsPublic,
                                  ItemCount = _context.Items.Count(i => i.InventoryId == p.Id),
                                  Tags = (from t in _context.Tags
                                          join it in _context.InventoryTags on t.Id equals it.TagId
                                          join inv in _context.Inventories on it.InventoryId equals inv.Id
                                          where it.InventoryId == p.Id
                                          select new Tag { Id = t.Id, Name = t.Name }).ToList()
                              };

            var result = await inventories
                .AsNoTracking()
                .OrderByDescending(x => x.ItemCount)
                .ThenBy(x => x.Title)
                .Take(5)
                .ToListAsync(cancellationToken);

            return Result.Success(result);

        }
    }
}
