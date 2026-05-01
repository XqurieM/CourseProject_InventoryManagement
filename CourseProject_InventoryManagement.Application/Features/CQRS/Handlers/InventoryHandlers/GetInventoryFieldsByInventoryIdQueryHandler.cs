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
    public class GetInventoryFieldsByInventoryIdQueryHandler : ICQRS.IGetInventoryFieldsByInventoryId
    {
        IAppDbContext _context;

        public GetInventoryFieldsByInventoryIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<GetInventoryFieldsByInventoryIdResult>>> GetInventoryFieldsByInventoryId(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var result = await _context.InventoryFields.Where(x => x.InventoryId == inventoryId && !x.IsDeleted)
                .Select(x => new GetInventoryFieldsByInventoryIdResult
                {
                    Id = x.Id,
                    InventoryId = x.InventoryId,
                    Name = x.Name,
                    Description = x.Description,
                    FieldType = x.FieldType,
                    DisplayOrder = x.DisplayOrder,
                    IsRequired = x.IsRequired,
                    ShowInTable = x.ShowInTable,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId,
                    UpdatedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
                    UpdatedByUserId = x.UpdatedByUserId ?? x.CreatedByUserId,
                    IsDeleted = x.IsDeleted,
                    DeletedAtUtc = x.DeletedAtUtc ?? x.CreatedAtUtc,
                }).ToListAsync(cancellationToken);
            return result;
        }
    }
}
