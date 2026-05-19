using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class UpdateInventoryTagsCommandHandler : ICQRS.IUpdateInventoryTags
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public UpdateInventoryTagsCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> UpdateInventoryTags(UpdateInventoryTagsCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var canManage = await _inventoryAuthorizationService.CanManageInventoryAsync(command.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<Guid>.Forbidden("Only the inventory owner or an admin can update inventory tags.");
            }

            var inventoryExists = await _context.Inventories.AnyAsync(x => x.Id == command.InventoryId && !x.IsDeleted, cancellationToken);
            if (!inventoryExists)
            {
                return Result<Guid>.NotFound("Inventory not found.");
            }

            var tagIds = command.TagIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (tagIds.Count > 0)
            {
                var tagCount = await _context.Tags.CountAsync(x => tagIds.Contains(x.Id), cancellationToken);
                if (tagCount != tagIds.Count)
                {
                    return Result<Guid>.NotFound("One or more tags could not be found.");
                }
            }

            var existingMappings = await _context.InventoryTags
                .Where(x => x.InventoryId == command.InventoryId)
                .ToListAsync(cancellationToken);

            if (existingMappings.Count > 0)
            {
                _context.InventoryTags.RemoveRange(existingMappings);
            }

            var newMappings = tagIds.Select(tagId => new InventoryTag
            {
                InventoryId = command.InventoryId,
                TagId = tagId
            }).ToList();

            if (newMappings.Count > 0)
            {
                await _context.InventoryTags.AddRangeAsync(newMappings, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(command.InventoryId);
        }
    }
}
