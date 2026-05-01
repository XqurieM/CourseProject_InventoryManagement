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
    public class GetMyEditableInventoriesQueryHandler : ICQRS.IGetMyEditableInventories
    {
        private readonly IAppDbContext _context;

        public GetMyEditableInventoriesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<GetProfileInventoriesResult>>> GetMyEditableInventories(Guid UserId, CancellationToken cancellationToken = default)
        {
            var result = from p in _context.Inventories
                         join u in _context.Users on p.CreatedByUserId equals u.Id
                         join i in _context.InventoryAccesses on p.Id equals i.InventoryId
                         where i.UserId == UserId
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
