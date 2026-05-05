using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class GetInventoryAccessListQueryHandler : ICQRS.IGetInventoryAccessList
    {
        IAppDbContext _context;

        public GetInventoryAccessListQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<InventoryAccessListDto>>> GetInventoryAccessList(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var accessList = _context.InventoryAccesses.Where(x => x.InventoryId == inventoryId)
                .Select(x => new InventoryAccessListDto
                {
                    Id = x.Id,
                    InventoryId = x.InventoryId,
                    UserId = x.UserId,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId,
                    UpdatedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
                    UpdatedByUserId = x.UpdatedByUserId ?? x.CreatedByUserId
                }).ToListAsync(cancellationToken);

            return Result<List<InventoryAccessListDto>>.Success(await accessList);
        }
    }
}
