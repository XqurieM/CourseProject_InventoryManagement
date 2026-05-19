using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Notifications;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CommentCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.CommentHandlers
{
    public class UpdateCommentCommandHandler : ICQRS.IUpdateComment
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;
        private readonly ICommentNotificationService _commentNotificationService;

        public UpdateCommentCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService,
            ICommentNotificationService commentNotificationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
            _commentNotificationService = commentNotificationService;
        }

        public async Task<Result<Guid>> UpdateComment(UpdateCommentCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            if (string.IsNullOrWhiteSpace(command.Content))
            {
                return Result<Guid>.Invalid(new ValidationError(nameof(command.Content), "Comment content is required."));
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(x => x.Id == command.CommentId && !x.IsDeleted, cancellationToken);

            if (comment is null)
            {
                return Result<Guid>.NotFound("Comment not found.");
            }

            var canComment = await _inventoryAuthorizationService.CanCommentOnInventoryAsync(comment.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canComment)
            {
                return Result<Guid>.Forbidden("You do not have permission to update comments on this inventory.");
            }

            if (comment.CreatedByUserId != userResult.Value.Id && !userResult.Value.IsAdmin)
            {
                return Result<Guid>.Forbidden("Only the comment owner or an admin can update this comment.");
            }

            comment.Content = command.Content.Trim();
            comment.UpdatedAtUtc = DateTime.UtcNow;
            comment.UpdatedByUserId = userResult.Value.Id;

            await _context.SaveChangesAsync(cancellationToken);
            await _commentNotificationService.SendCommentUpdatedNotificationAsync(
                comment.InventoryId,
                comment.Id,
                comment.Content,
                userResult.Value.Id,
                userResult.Value.UserName,
                comment.UpdatedAtUtc ?? DateTime.UtcNow,
                cancellationToken);
            return Result<Guid>.Success(comment.Id);
        }
    }
}
