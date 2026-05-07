using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CategoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.CategoryHandlers
{
    public class UpdateCategoryCommandHandler : ICQRS.IUpdateCategory
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IReferenceDataAuthorizationService _referenceDataAuthorizationService;

        public UpdateCategoryCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IReferenceDataAuthorizationService referenceDataAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _referenceDataAuthorizationService = referenceDataAuthorizationService;
        }

        public async Task<Result<Guid>> UpdateCategory(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var canManageCategories = await _referenceDataAuthorizationService.CanManageCategoriesAsync(userResult.Value.Id, cancellationToken);
            if (!canManageCategories)
            {
                return Result<Guid>.Forbidden("You do not have permission to manage categories.");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == command.CategoryId, cancellationToken);

            if (category is null)
            {
                return Result<Guid>.NotFound("Category not found.");
            }

            if (command.Name is not null)
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    return Result<Guid>.Invalid(new ValidationError(nameof(command.Name), "Name cannot be empty."));
                }

                category.Name = command.Name.Trim();
            }

            if (command.Description is not null)
            {
                category.Description = command.Description.Trim();
            }

            if (command.IsActive.HasValue)
            {
                category.IsActive = command.IsActive.Value;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(category.Id);
        }
    }
}
