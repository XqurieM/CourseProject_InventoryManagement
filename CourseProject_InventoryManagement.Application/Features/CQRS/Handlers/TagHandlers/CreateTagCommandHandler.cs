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
    public class CreateTagCommandHandler : ICQRS.ICreateTag
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IReferenceDataAuthorizationService _referenceDataAuthorizationService;

        public CreateTagCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IReferenceDataAuthorizationService referenceDataAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _referenceDataAuthorizationService = referenceDataAuthorizationService;
        }

        public async Task<Result<Guid>> CreateTag(CreateTagCommand command, CancellationToken cancellationToken = default)
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

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return Result<Guid>.Invalid(new ValidationError(nameof(command.Name), "Tag name is required."));
            }

            var normalizedName = command.Name.Trim();
            var canonicalName = normalizedName.ToUpperInvariant();

            var exists = await _context.Tags.AnyAsync(x => x.NormalizedName == canonicalName, cancellationToken);
            if (exists)
            {
                return Result<Guid>.Conflict("This tag already exists.");
            }

            var tag = new Tag
            {
                Name = normalizedName,
                NormalizedName = canonicalName
            };

            await _context.Tags.AddAsync(tag, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(tag.Id);
        }
    }
}
