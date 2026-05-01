using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.ItemResults;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class GetItemFieldValuesByInventoryIdQueryHandler : ICQRS.IGetItemFieldValuesByInventoryId
    {
        private readonly IAppDbContext _context;

        public GetItemFieldValuesByInventoryIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ItemFieldValuesResult>>> GetItemFieldValuesByInventoryId(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var result = await (from p in _context.ItemFieldValues
                                join i in _context.Items on p.ItemId equals i.Id
                                where i.InventoryId == inventoryId
                                select new ItemFieldValuesResult
                                {
                                    Id = p.Id,
                                    ItemId = p.ItemId,
                                    InventoryFieldId = p.InventoryFieldId,
                                    StringValue = p.StringValue ?? string.Empty,
                                    NumberValue = p.NumberValue ?? 0,
                                    BooleanValue = p.BooleanValue ?? false,
                                    CreatedAtUtc = p.CreatedAtUtc,
                                    CreatedByUserId = p.CreatedByUserId,
                                    UpdatedAtUtc = p.UpdatedAtUtc ?? p.CreatedAtUtc,
                                    UpdatedByUserId = p.UpdatedByUserId ?? p.CreatedByUserId
                                }).ToListAsync(cancellationToken);

            return result;
        }
    }
}
