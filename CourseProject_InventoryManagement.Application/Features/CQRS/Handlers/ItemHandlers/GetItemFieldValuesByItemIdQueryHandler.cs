using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.ItemResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class GetItemFieldValuesByItemIdQueryHandler : ICQRS.IGetItemFieldValuesByItemId
    {
        private readonly IAppDbContext _context;

        public GetItemFieldValuesByItemIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ItemFieldValuesResult>>> GetItemFieldValuesByItemId(Guid itemId, CancellationToken cancellationToken = default)
        {
            var result = await _context.ItemFieldValues
                .Where(x => x.ItemId == itemId)
                .Select(x => new ItemFieldValuesResult
                {
                    Id = x.Id,
                    ItemId = x.ItemId,
                    InventoryFieldId = x.InventoryFieldId,
                    StringValue = x.StringValue ?? string.Empty,
                    NumberValue = x.NumberValue ?? 0,
                    BooleanValue = x.BooleanValue ?? false,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId,
                    UpdatedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
                    UpdatedByUserId = x.UpdatedByUserId ?? x.CreatedByUserId
                })
                .ToListAsync(cancellationToken);
            return result;
        }
    }
}
