using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class UpdateItemImagesCommandHandler : ICQRS.IUpdateItemImages
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public UpdateItemImagesCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> UpdateItemImages(UpdateItemImagesCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(x => x.Id == command.ItemId && !x.IsDeleted, cancellationToken);

            if (item is null)
            {
                return Result<Guid>.NotFound("Item not found.");
            }

            var canUpdate = await _inventoryAuthorizationService.CanUpdateItemsAsync(item.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canUpdate)
            {
                return Result<Guid>.Forbidden("You do not have permission to update item images.");
            }

            var normalizedImages = (command.Images ?? [])
                .Where(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                .Select((x, index) => new
                {
                    ImageUrl = x.ImageUrl.Trim(),
                    Caption = string.IsNullOrWhiteSpace(x.Caption) ? null : x.Caption.Trim(),
                    DisplayOrder = x.DisplayOrder > 0 ? x.DisplayOrder : index + 1,
                    x.IsPrimary
                })
                .ToList();

            if (normalizedImages.Select(x => x.ImageUrl).Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalizedImages.Count)
            {
                return Result<Guid>.Invalid(new ValidationError(nameof(command.Images), "Duplicate image URLs are not allowed for the same item."));
            }

            if (normalizedImages.Count > 0 && normalizedImages.All(x => !x.IsPrimary))
            {
                normalizedImages[0] = new
                {
                    normalizedImages[0].ImageUrl,
                    normalizedImages[0].Caption,
                    normalizedImages[0].DisplayOrder,
                    IsPrimary = true
                };
            }

            var existingImages = await _context.ItemImages
                .Where(x => x.ItemId == command.ItemId)
                .ToListAsync(cancellationToken);

            if (existingImages.Count > 0)
            {
                _context.ItemImages.RemoveRange(existingImages);
            }

            if (normalizedImages.Count > 0)
            {
                var now = DateTime.UtcNow;
                var newImages = normalizedImages
                    .OrderBy(x => x.DisplayOrder)
                    .Select((x, index) => new ItemImage
                    {
                        Id = Guid.NewGuid(),
                        ItemId = command.ItemId,
                        ImageUrl = x.ImageUrl,
                        Caption = x.Caption,
                        DisplayOrder = index + 1,
                        IsPrimary = x.IsPrimary,
                        CreatedByUserId = userResult.Value.Id,
                        CreatedAtUtc = now
                    })
                    .ToList();

                // Keep a single primary image even if the UI sends more than one.
                var firstPrimary = newImages.FindIndex(x => x.IsPrimary);
                if (firstPrimary < 0)
                {
                    newImages[0].IsPrimary = true;
                }
                else
                {
                    for (var i = 0; i < newImages.Count; i++)
                    {
                        newImages[i].IsPrimary = i == firstPrimary;
                    }
                }

                await _context.ItemImages.AddRangeAsync(newImages, cancellationToken);
            }

            item.UpdatedAtUtc = DateTime.UtcNow;
            item.UpdatedByUserId = userResult.Value.Id;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(item.Id);
        }
    }
}
