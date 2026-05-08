using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class ToggleItemLikeCommandHandler : ICQRS.IToggleItemLike
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public ToggleItemLikeCommandHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Result<bool>> ToggleItemLike(ToggleItemLikeCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return Result<bool>.Unauthorized();
            }

            var userId = userResult.Value.Id;

            var itemExists = await _context.Items.AnyAsync(x => x.Id == command.ItemId && !x.IsDeleted, cancellationToken);
            if (!itemExists)
            {
                return Result<bool>.NotFound("Item not found.");
            }

            var existingLike = await _context.ItemLikes
                .FirstOrDefaultAsync(x => x.ItemId == command.ItemId && x.CreatedByUserId == userId, cancellationToken);

            bool isLiked;

            if (existingLike != null)
            {
                // Zaten beğenilmiş, demek ki geri çekiyoruz (Unlike)
                _context.ItemLikes.Remove(existingLike);
                isLiked = false;
            }
            else
            {
                // Henüz beğenilmemiş, beğeniyoruz (Like)
                var newLike = new ItemLike
                {
                    Id = Guid.NewGuid(),
                    ItemId = command.ItemId,
                    CreatedByUserId = userId,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _context.ItemLikes.AddAsync(newLike, cancellationToken);
                isLiked = true;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(isLiked);
        }
    }
}
