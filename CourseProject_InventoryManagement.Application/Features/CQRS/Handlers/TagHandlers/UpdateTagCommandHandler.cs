using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.TagCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.TagHandlers
{
    public class UpdateTagCommandHandler : ICQRS.IUpdateTag
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IReferenceDataAuthorizationService _referenceDataAuthorizationService;

        public UpdateTagCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IReferenceDataAuthorizationService referenceDataAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _referenceDataAuthorizationService = referenceDataAuthorizationService;
        }

        public async Task<Result<Guid>> UpdateTag(UpdateTagCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var canManageTags = await _referenceDataAuthorizationService.CanManageTagsAsync(userResult.Value.Id, cancellationToken);
            if (!canManageTags)
            {
                return Result<Guid>.Forbidden("You do not have permission to manage tags.");
            }

            var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == command.TagId, cancellationToken);
            if (tag is null)
            {
                return Result<Guid>.NotFound("Tag not found.");
            }

            if (command.Name is not null)
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    return Result<Guid>.Invalid(new ValidationError(nameof(command.Name), "Tag name cannot be empty."));
                }

                var normalizedName = command.Name.Trim();
                var canonicalName = normalizedName.ToUpperInvariant();
                var duplicateExists = await _context.Tags.AnyAsync(x => x.Id != command.TagId && x.NormalizedName == canonicalName, cancellationToken);
                if (duplicateExists)
                {
                    return Result<Guid>.Conflict("This tag name is already in use.");
                }

                tag.Name = normalizedName;
                tag.NormalizedName = canonicalName;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(tag.Id);
        }
    }
}
