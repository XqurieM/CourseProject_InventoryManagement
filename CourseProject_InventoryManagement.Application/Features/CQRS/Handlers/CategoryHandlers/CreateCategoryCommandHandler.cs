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
    public class CreateCategoryCommandHandler : ICQRS.ICreateCategory
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IReferenceDataAuthorizationService _referenceDataAuthorizationService;

        public CreateCategoryCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IReferenceDataAuthorizationService referenceDataAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _referenceDataAuthorizationService = referenceDataAuthorizationService;
        }

        public async Task<Result<Guid>> CreateCategory(CreateCategoryCommand command, CancellationToken cancellationToken = default)
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

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return Result<Guid>.Invalid(new ValidationError
                {
                    Identifier = nameof(command.Name),
                    ErrorMessage = "Category name is required."
                });
            }

            if (!command.IsActive.HasValue)
            {
                return Result<Guid>.Invalid(new ValidationError
                {
                    Identifier = nameof(command.IsActive),
                    ErrorMessage = "IsActive value is required."
                });
            }

            var normalizedName = command.Name.Trim();
            var categoryExists = await _context.Categories
                .AnyAsync(x => x.Name == normalizedName && x.IsActive, cancellationToken);

            if (categoryExists)
            {
                return Result<Guid>.Conflict("This category already exists.");
            }

            var category = new Category
            {
                Name = normalizedName,
                Description = command.Description?.Trim(),
                IsActive = command.IsActive.Value
            };

            await _context.Categories.AddAsync(category, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(category.Id);
        }
    }
}
