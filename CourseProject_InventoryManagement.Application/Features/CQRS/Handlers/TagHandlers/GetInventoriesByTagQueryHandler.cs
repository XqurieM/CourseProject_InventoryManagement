using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.TagQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.TagHandlers
{
    public class GetInventoriesByTagQueryHandler : ICQRS.IGetInventoriesByTag
    {
        private readonly IAppDbContext _context;

        public GetInventoriesByTagQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<GetInventoriesWithJoinInfosResult>>> GetInventoriesByTag(GetInventoriesByTagQuery query, CancellationToken cancellationToken = default)
        {
            var tagExists = await _context.Tags.AnyAsync(x => x.Id == query.TagId, cancellationToken);
            if (!tagExists)
            {
                return Result<List<GetInventoriesWithJoinInfosResult>>.NotFound("Tag not found.");
            }

            var inventoriesQuery =
                from inventory in _context.Inventories
                join user in _context.Users on inventory.CreatedByUserId equals user.Id
                where !inventory.IsDeleted &&
                      inventory.InventoryTags.Any(it => it.TagId == query.TagId)
                select new { inventory, user };

            if (query.UserId.HasValue)
            {
                var userId = query.UserId.Value;
                inventoriesQuery = inventoriesQuery.Where(x =>
                    x.inventory.IsPublic ||
                    x.inventory.CreatedByUserId == userId ||
                    x.inventory.Accesses.Any(a => a.UserId == userId));
            }
            else
            {
                inventoriesQuery = inventoriesQuery.Where(x => x.inventory.IsPublic);
            }

            var inventories = await inventoriesQuery
                .OrderByDescending(x => x.inventory.CreatedAtUtc)
                .Select(x => new GetInventoriesWithJoinInfosResult
                {
                    Id = x.inventory.Id,
                    Title = x.inventory.Title,
                    Description = x.inventory.Description,
                    CategoryId = x.inventory.CategoryId,
                    CategoryName = x.inventory.Category.Name,
                    IsPublic = x.inventory.IsPublic,
                    UserName = x.user.UserName,
                    ImageUrl = x.inventory.ImageUrl,
                    ItemCount = x.inventory.Items.Count(i => !i.IsDeleted),
                    Tags = x.inventory.InventoryTags
                        .Select(it => new Tag
                        {
                            Id = it.Tag.Id,
                            Name = it.Tag.Name
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return Result<List<GetInventoriesWithJoinInfosResult>>.Success(inventories);
        }
    }
}
