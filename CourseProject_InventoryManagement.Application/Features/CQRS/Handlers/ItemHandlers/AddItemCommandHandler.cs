using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using CourseProject_InventoryManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers
{
    public class AddItemCommandHandler : ICQRS.IAddItem
    {
        private readonly IAppDbContext _context;
        private readonly ICustomIdGenerator _idGenerator;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public AddItemCommandHandler(
            IAppDbContext context,
            ICustomIdGenerator idGenerator,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _idGenerator = idGenerator;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<List<Guid>>> AddItem(AddItemCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<Guid>>(userResult);
            }

            var inventoryExists = await _context.Inventories.AnyAsync(x => x.Id == command.InventoryId, cancellationToken);
            if (!inventoryExists)
            {
                return Result<List<Guid>>.NotFound($"The inventory with ID '{command.InventoryId}' was not found.");
            }

            if (command.Items == null || !command.Items.Any())
            {
                return Result<List<Guid>>.Invalid(new ValidationError("The item list cannot be empty."));
            }

            var canWriteItems = await _inventoryAuthorizationService.CanWriteItemsAsync(command.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canWriteItems)
            {
                return Result<List<Guid>>.Forbidden("You do not have permission to add items to this inventory.");
            }

            var rules = await _context.InventoryCustomIdRules
                .Where(x => x.InventoryId == command.InventoryId)
                .OrderBy(x => x.PartOrder)
                .ToListAsync(cancellationToken);

            var currentCount = await _context.Items
                .CountAsync(x => x.InventoryId == command.InventoryId, cancellationToken);

            var newItems = new List<Item>();
            var now = DateTime.UtcNow;

            try
            {
                foreach (var itemDto in command.Items)
                {
                    var generatedCustomId = _idGenerator.Generate(rules, currentCount);
                    currentCount++;

                    var newItem = new Item
                    {
                        Id = Guid.NewGuid(),
                        ItemName = itemDto.ItemName,
                        InventoryId = command.InventoryId,
                        CustomId = generatedCustomId,
                        CreatedAtUtc = now,
                        CreatedByUserId = userResult.Value.Id,
                        IsDeleted = false
                    };

                    newItems.Add(newItem);
                }

                await _context.Items.AddRangeAsync(newItems, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return Result<List<Guid>>.Success(newItems.Select(x => x.Id).ToList());
            }
            catch (Exception ex)
            {
                return Result<List<Guid>>.Error($"An error occurred during bulk item insertion: {ex.Message}");
            }
        }
    }
}
