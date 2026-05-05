using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class GetItemByIdQueryHandler : ICQRS.IGetItemById
    {
        private readonly IAppDbContext _context;

        public GetItemByIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ItemDto>> GetItemById(Guid itemId, CancellationToken cancellationToken = default)
        {
            var items = (from p in _context.Items
                        join j in _context.Inventories on p.InventoryId equals j.Id
                        where p.Id == itemId
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
                        }).FirstOrDefault();



            return items is not null ? Result.Success(items) : Result.NotFound();
        }
    }
}
