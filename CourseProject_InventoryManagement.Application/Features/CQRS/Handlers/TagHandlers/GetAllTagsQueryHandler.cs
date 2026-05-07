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
    public class GetAllTagsQueryHandler : ICQRS.IGetAllTags
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IReferenceDataAuthorizationService _referenceDataAuthorizationService;

        public GetAllTagsQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IReferenceDataAuthorizationService referenceDataAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _referenceDataAuthorizationService = referenceDataAuthorizationService;
        }

        public async Task<Result<List<TagDto>>> GetAllTags(CancellationToken cancellationToken = default)
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
