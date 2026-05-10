using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.TagHandlers
{
    public class GetTagByIdQueryHandler : ICQRS.IGetTagById
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IReferenceDataAuthorizationService _referenceDataAuthorizationService;

        public GetTagByIdQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IReferenceDataAuthorizationService referenceDataAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _referenceDataAuthorizationService = referenceDataAuthorizationService;
        }

        public async Task<Result<TagDto>> GetTagById(Guid tagId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, TagDto>(userResult);
            }

            var canReadTags = await _referenceDataAuthorizationService.CanReadTagsAsync(userResult.Value.Id, cancellationToken);
            if (!canReadTags)
            {
                return Result<TagDto>.Forbidden("You do not have permission to view tags.");
            }

            var tag = await _context.Tags
                .Where(x => x.Id == tagId)
                .Select(x => new TagDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    NormalizedName = x.NormalizedName,
                    InventoryCount = x.InventoryTags.Count()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (tag is null)
            {
                return Result<TagDto>.NotFound("Tag not found.");
            }

            return Result<TagDto>.Success(tag);
        }
    }
}
