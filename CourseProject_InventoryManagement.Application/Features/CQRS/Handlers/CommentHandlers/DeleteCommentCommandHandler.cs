using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.CommentHandlers
{
    public class DeleteCommentCommandHandler : ICQRS.IDeleteComment
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public DeleteCommentCommandHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService, IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> DeleteComment(Guid commentId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var commentExists = await _context.Comments.Where(x => x.Id == commentId && !x.IsDeleted).FirstOrDefaultAsync(cancellationToken);
            if (commentExists == null || commentExists.IsDeleted)
            {
                return Result<Guid>.NotFound($"The comment with ID '{commentId}' was not found.");
            }   

            var canComment = await _inventoryAuthorizationService.CanCommentOnInventoryAsync(commentExists.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canComment)
            {
                return Result<Guid>.Forbidden("You do not have permission to comment on this inventory.");
            }
            DateTime utcNow = DateTime.UtcNow;
            commentExists.IsDeleted = true;
            commentExists.DeletedAtUtc = utcNow;
            commentExists.UpdatedAtUtc = utcNow;
            commentExists.UpdatedByUserId = userResult.Value.Id;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(commentId);
        }
    }
}
