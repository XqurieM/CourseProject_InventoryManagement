using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.TagQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.TagHandlers
{
    public class GetPopularTagsQueryHandler : ICQRS.IGetPopularTags
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IReferenceDataAuthorizationService _referenceDataAuthorizationService;

        public GetPopularTagsQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IReferenceDataAuthorizationService referenceDataAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _referenceDataAuthorizationService = referenceDataAuthorizationService;
        }

        public async Task<Result<List<TagDto>>> GetPopularTags(GetPopularTagsQuery query, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<TagDto>>(userResult);
            }

            var canReadTags = await _referenceDataAuthorizationService.CanReadTagsAsync(userResult.Value.Id, cancellationToken);
            if (!canReadTags)
            {
                return Result<List<TagDto>>.Forbidden("You do not have permission to view tags.");
            }

            var take = query.Take <= 0 ? 20 : Math.Min(query.Take, 100);
            var tags = await _context.Tags
                .Select(tag => new TagDto
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    NormalizedName = tag.NormalizedName,
                    InventoryCount = tag.InventoryTags.Count()
                })
                .OrderByDescending(x => x.InventoryCount)
                .ThenBy(x => x.Name)
                .Take(take)
                .ToListAsync(cancellationToken);

            return Result<List<TagDto>>.Success(tags);
        }
    }
}
