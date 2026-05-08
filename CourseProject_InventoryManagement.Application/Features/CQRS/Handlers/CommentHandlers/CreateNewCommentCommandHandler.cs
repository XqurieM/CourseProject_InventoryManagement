using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CommentCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Application.Abstractions.Notifications;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.CommentHandlers
{
    public class CreateNewCommentCommandHandler : ICQRS.ICreateNewComment
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;
        private readonly ICommentNotificationService _commentNotificationService;

        public CreateNewCommentCommandHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService, IInventoryAuthorizationService inventoryAuthorizationService, ICommentNotificationService commentNotificationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
            _commentNotificationService = commentNotificationService;
        }

        public async Task<Result<Guid>> CreateNewComment(CreateNewCommentCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var inventoryExists = await _context.Inventories.AnyAsync(x => x.Id == command.InventoryId && !x.IsDeleted, cancellationToken);
            if (!inventoryExists)
            {
                return Result<Guid>.NotFound($"The inventory with ID '{command.InventoryId}' was not found.");
            }

            var canComment = await _inventoryAuthorizationService.CanCommentOnInventoryAsync(command.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canComment)
            {
                return Result<Guid>.Forbidden("You do not have permission to comment on this inventory.");
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                InventoryId = command.InventoryId,
                Content = command.Content,
                CreatedByUserId = userResult.Value.Id,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _context.Comments.AddAsync(comment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            await _commentNotificationService.SendCommentAddedNotificationAsync(
                command.InventoryId,
                comment.Id,
                comment.Content,
                userResult.Value.Id,
                userResult.Value.UserName,
                comment.CreatedAtUtc,
                cancellationToken);

            return Result<Guid>.Created(comment.Id);
        }
    }
}
