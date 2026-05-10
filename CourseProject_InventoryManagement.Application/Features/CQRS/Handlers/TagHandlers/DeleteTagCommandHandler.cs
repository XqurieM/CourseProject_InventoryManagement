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
    public class DeleteTagCommandHandler : ICQRS.IDeleteTag
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IReferenceDataAuthorizationService _referenceDataAuthorizationService;

        public DeleteTagCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IReferenceDataAuthorizationService referenceDataAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _referenceDataAuthorizationService = referenceDataAuthorizationService;
        }

        public async Task<Result<Guid>> DeleteTag(DeleteTagCommand command, CancellationToken cancellationToken = default)
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

            var relations = await _context.InventoryTags.Where(x => x.TagId == command.TagId).ToListAsync(cancellationToken);
            if (relations.Count > 0)
            {
                _context.InventoryTags.RemoveRange(relations);
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(command.TagId);
        }
    }
}
