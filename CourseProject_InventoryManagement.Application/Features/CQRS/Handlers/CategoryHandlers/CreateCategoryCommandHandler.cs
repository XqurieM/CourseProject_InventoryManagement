using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CategoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.CategoryHandlers
{
    public class CreateCategoryCommandHandler : ICQRS.ICreateCategory
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public CreateCategoryCommandHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Result<Guid>> CreateCategory(CreateCategoryCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
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

            var categoryExists = await _context.Categories
                .AnyAsync(x => x.Name == command.Name && x.IsActive, cancellationToken);

            if (categoryExists)
            {
                return Result<Guid>.NotFound("Category found. You can not add this category");
            }

            var category = new Category
            {
                Name = command.Name.Trim(),
                Description = command.Description?.Trim(),
                IsActive = command.IsActive.Value                
            };

            await _context.Categories.AddAsync(category, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(category.Id);
        }
    }
}
