using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class GetMyOwnInventoriesQueryHandler : ICQRS.IGetOwnInventories
    {
        IAppDbContext _context;

        public GetMyOwnInventoriesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<GetProfileInventoriesResult>>> GetOwnInventories(Guid UserId, CancellationToken cancellationToken = default)
        {
            var result = from p in _context.Inventories
                         join u in _context.Users on p.CreatedByUserId equals u.Id                         
                         where p.CreatedByUserId == UserId
                         select new GetProfileInventoriesResult
                         {
                             Id = p.Id,
                             Title = p.Title,
                             CategoryId = p.CategoryId,
                             CategoryName = p.Category.Name,
                             Description = p.Description,
                             IsPublic = p.IsPublic,
                             ItemCount = _context.Items.Count(x => x.InventoryId == p.Id),
                             UserName = u.UserName,
                             CreatedByUserId = u.Id,
                             UpdateDate = p.UpdatedAtUtc ?? p.CreatedAtUtc
                         };

            return Result.Success(await result.ToListAsync(cancellationToken));
        }
    }
}
