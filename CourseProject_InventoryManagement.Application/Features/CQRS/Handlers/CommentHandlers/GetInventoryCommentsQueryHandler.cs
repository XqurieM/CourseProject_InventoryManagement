using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
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
    public class GetInventoryCommentsQueryHandler : ICQRS.IGetInventoryComments
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public GetInventoryCommentsQueryHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService, IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<List<CommentDto>>> GetInventoryComments(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<CommentDto>>(userResult);
            }

            var inventoryExists = await _context.Inventories.AnyAsync(x => x.Id == inventoryId && !x.IsDeleted, cancellationToken);
            if (!inventoryExists)
            {
                return Result<List<CommentDto>>.NotFound($"The inventory with ID '{inventoryId}' was not found.");
            }

            var canComment = await _inventoryAuthorizationService.CanCommentOnInventoryAsync(inventoryId, userResult.Value.Id, cancellationToken);
            if (!canComment)
            {
                return Result<List<CommentDto>>.Forbidden("You do not have permission to comment on this inventory.");
            }

            var comments = await _context.Comments
                .Where(c => c.InventoryId == inventoryId && !c.IsDeleted)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    InventoryId = c.InventoryId,
                    Content = c.Content,
                    CreatedAtUtc = c.CreatedAtUtc,
                    CreatedByUserId = c.CreatedByUserId,
                    UpdatedAtUtc = c.UpdatedAtUtc ?? c.CreatedAtUtc,
                    UpdatedByUserId = c.UpdatedByUserId ?? c.CreatedByUserId,
                    IsDeleted = c.IsDeleted,
                    DeletedAtUtc = c.DeletedAtUtc ?? c.CreatedAtUtc,
                })
                .ToListAsync(cancellationToken);

            return Result<List<CommentDto>>.Success(comments);
        }
    }
}
