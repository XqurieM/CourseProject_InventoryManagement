using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.TagQueries;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.TagHandlers
{
    public class SearchTagsQueryHandler : ICQRS.ISearchTags
    {
        private readonly IAppDbContext _context;

        public SearchTagsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<TagDto>>> SearchTags(SearchTagsQuery query, CancellationToken cancellationToken = default)
        {
            var tagQuery = _context.Tags.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.Trim().ToLower();
                tagQuery = tagQuery.Where(x => x.Name.ToLower().Contains(searchTerm) || x.NormalizedName.ToLower().Contains(searchTerm));
            }

            var tags = await tagQuery
                .Select(x => new TagDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    NormalizedName = x.NormalizedName,
                    InventoryCount = x.InventoryTags.Count()
                })
                .OrderByDescending(x => x.InventoryCount)
                .ThenBy(x => x.Name)
                .Take(20)
                .ToListAsync(cancellationToken);

            return Result<List<TagDto>>.Success(tags);
        }
    }
}
