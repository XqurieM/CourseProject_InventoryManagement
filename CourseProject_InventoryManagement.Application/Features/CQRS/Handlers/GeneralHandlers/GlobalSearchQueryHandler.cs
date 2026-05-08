using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.GeneralHandlers
{
    public class GlobalSearchQueryHandler : ICQRS.IGlobalSearch
    {
        private readonly IAppDbContext _context;

        public GlobalSearchQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<GlobalSearchResult>> GlobalSearch(GlobalSearchQuery query, CancellationToken cancellationToken = default)
        {
            var result = new GlobalSearchResult();

            if (string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                return Result<GlobalSearchResult>.Success(result);
            }

            var searchTerm = query.SearchTerm.ToLower();

            var inventoriesQuery = _context.Inventories
                .Include(x => x.Category)
                .Where(x => !x.IsDeleted && 
                            (x.Title.ToLower().Contains(searchTerm) || 
                            (x.Description != null && x.Description.ToLower().Contains(searchTerm)) ||
                            x.InventoryTags.Any(t => t.Tag.Name.ToLower().Contains(searchTerm)) ||
                            x.Comments.Any(c => !c.IsDeleted && c.Content.ToLower().Contains(searchTerm))));

            if (query.UserId.HasValue)
            {
                var userId = query.UserId.Value;
                inventoriesQuery = inventoriesQuery.Where(x => x.IsPublic || x.CreatedByUserId == userId);
            }
            else
            {
                inventoriesQuery = inventoriesQuery.Where(x => x.IsPublic);
            }

            var inventories = await inventoriesQuery
                .Take(20)
                .Select(x => new InventoryDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    IsPublic = x.IsPublic,
                    ImageUrl = x.ImageUrl,
                    RowVersion = x.RowVersion,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId,
                    CanManageInventory = query.UserId.HasValue && x.CreatedByUserId == query.UserId.Value,
                    CanWriteItems = query.UserId.HasValue && (x.IsPublic || x.CreatedByUserId == query.UserId.Value)
                })
                .ToListAsync(cancellationToken);

            var itemsQuery = _context.Items
                .Include(x => x.Inventory)
                .Where(x => !x.IsDeleted && !x.Inventory.IsDeleted &&
                            (x.ItemName.ToLower().Contains(searchTerm) || 
                            (x.CustomId != null && x.CustomId.ToLower().Contains(searchTerm)) ||
                            x.FieldValues.Any(fv => fv.StringValue != null && fv.StringValue.ToLower().Contains(searchTerm))));

            if (query.UserId.HasValue)
            {
                var userId = query.UserId.Value;
                itemsQuery = itemsQuery.Where(x => x.Inventory.IsPublic || x.Inventory.CreatedByUserId == userId);
            }
            else
            {
                itemsQuery = itemsQuery.Where(x => x.Inventory.IsPublic);
            }

            var items = await itemsQuery
                .Take(20)
                .Select(x => new ItemDto
                {
                    Id = x.Id,
                    InventoryId = x.InventoryId,
                    CustomId = x.CustomId ?? string.Empty,
                    RowVersion = x.RowVersion,
                    CreateAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId,
                    UpdatedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
                    UpdatedByUserId = x.UpdatedByUserId ?? x.CreatedByUserId,
                    IsDeleted = x.IsDeleted,
                    DeletedAtUtc = x.DeletedAtUtc ?? default,
                    ItemName = x.ItemName
                })
                .ToListAsync(cancellationToken);

            result.Inventories = inventories;
            result.Items = items;

            return Result<GlobalSearchResult>.Success(result);
        }
    }
}
