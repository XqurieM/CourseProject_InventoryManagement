using System;
using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class CreateInventoryCommandHandler : ICQRS.ICreateInventory
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public CreateInventoryCommandHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Result<Guid>> CreateInventory(CreateInventoryCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            if (string.IsNullOrWhiteSpace(command.Title))
            {
                return Result<Guid>.Invalid(new ValidationError
                {
                    Identifier = nameof(command.Title),
                    ErrorMessage = "Inventory title is required."
                });
            }

            var categoryExists = await _context.Categories
                .AnyAsync(x => x.Id == command.CategoryId && x.IsActive, cancellationToken);

            if (!categoryExists)
            {
                return Result<Guid>.NotFound("Category not found.");
            }

            var inventory = new Inventory
            {
                Title = command.Title.Trim(),
                Description = command.Description?.Trim(),
                CategoryId = command.CategoryId,
                ImageUrl = command.ImageUrl?.Trim(),
                IsPublic = command.IsPublic,
                CreatedByUserId = userResult.Value.Id,
                ApiToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")
            };

            await _context.Inventories.AddAsync(inventory, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Created(inventory.Id);
        }
    }
}
