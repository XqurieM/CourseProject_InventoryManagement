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
    public class GetLast10InventoriesQueryHandler : ICQRS.IGetLast10Inventories
    {
        IAppDbContext _context;
        public GetLast10InventoriesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }
        public async Task<Result<List<GetInventoriesWithJoinInfosResult>>> GetLast10Inventories(CancellationToken cancellationToken = default)
        {
            var inventories = from p in _context.Inventories
                              join u in _context.Users on p.CreatedByUserId equals u.Id
                              orderby p.CreatedAtUtc descending
                              select new GetInventoriesWithJoinInfosResult
                              {
                                  Id = p.Id,
                                  Title = p.Title,
                                  Description = p.Description,
                                  CategoryName = p.Category.Name,
                                  IsPublic = p.IsPublic,
                                  CreatedByUserId = p.CreatedByUserId,
                                  UserName = u.UserName,
                                  ItemCount = _context.Items.Count(i => i.InventoryId == p.Id),
                                  Tags = (from t in _context.Tags
                                          join it in _context.InventoryTags on t.Id equals it.TagId
                                          join inv in _context.Inventories on it.InventoryId equals inv.Id
                                          where it.InventoryId == p.Id
                                          select new Tag { Id = t.Id, Name = t.Name }).ToList()
                              };

            var result = await inventories
                .AsNoTracking()
                .Take(10)
                .ToListAsync(cancellationToken);

            return Result.Success(result);
        }
    }
}
