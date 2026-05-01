using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.TagHandlers
{
    public class GetAllTagsQueryHandler : ICQRS.IGetAllTags
    {
        IAppDbContext _context;

        public GetAllTagsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<TagDto>>> GetAllTags(CancellationToken cancellationToken = default)
        {
            var tags = await _context.Tags
                .Select(tag => new TagDto
                {
                    Name = tag.Name,
                    NormalizedName = tag.NormalizedName
                })
                .ToListAsync(cancellationToken);

            return Result<List<TagDto>>.Success(tags);
        }
    }
}
