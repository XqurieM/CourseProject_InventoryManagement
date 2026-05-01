using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class GetItemsByInventoryIdQueryHandler : ICQRS.IGetItemsByInventoryId
    {
        IAppDbContext _context;

        public GetItemsByInventoryIdQueryHandler(IAppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task<Result<List<ItemDto>>> GetItemsByInventoryId(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var items = from p in _context.Items
                        join j in _context.Inventories on p.InventoryId equals j.Id
                        select new ItemDto
                        {
                            Id = p.Id,
                            InventoryId = j.Id,
                            CustomId = p.CustomId,
                            RowVersion = p.RowVersion,
                            CreateAtUtc = p.CreatedAtUtc,
                            CreatedByUserId = p.CreatedByUserId,
                            UpdatedAtUtc = p.UpdatedAtUtc ?? DateTime.MinValue,
                            UpdatedByUserId = p.UpdatedByUserId ?? p.CreatedByUserId,
                            IsDeleted = p.IsDeleted,
                            DeletedAtUtc = p.DeletedAtUtc ?? DateTime.MinValue,
                            ItemName = p.ItemName
                        };



            return Result.Success(await items.ToListAsync(cancellationToken));
        }
    }
}
