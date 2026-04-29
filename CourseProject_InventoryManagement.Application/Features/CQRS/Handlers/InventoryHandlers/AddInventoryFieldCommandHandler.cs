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
    public class AddInventoryFieldCommandHandler : ICQRS.IAddInventoryField
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IInventoryAuthorizationService _inventoryAuthorizationService;

        public AddInventoryFieldCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IInventoryAuthorizationService inventoryAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _inventoryAuthorizationService = inventoryAuthorizationService;
        }

        public async Task<Result<Guid>> AddInventoryField(AddInventoryFieldCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(userResult);
            }

            var canManage = await _inventoryAuthorizationService.CanManageInventoryAsync(command.InventoryId, userResult.Value.Id, cancellationToken);
            if (!canManage)
            {
                return Result<Guid>.Forbidden("Only the inventory owner or an admin can edit inventory fields.");
            }

            var existingFields = await _context.InventoryFields
                .Where(x => x.InventoryId == command.InventoryId)
                .ToListAsync(cancellationToken);

            if (existingFields.Any())
            {
                _context.InventoryFields.RemoveRange(existingFields);
            }

            var newFields = command.Fields.Select(f => new InventoryField
            {
                Id = Guid.NewGuid(),
                InventoryId = command.InventoryId,
                Name = f.Name,
                Description = f.Description,
                FieldType = f.FieldType,
                DisplayOrder = f.DisplayOrder,
                IsRequired = f.IsRequired,
                ShowInTable = f.ShowInTable,
                CreatedByUserId = userResult.Value.Id
            }).ToList();

            await _context.InventoryFields.AddRangeAsync(newFields, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(command.InventoryId);
        }
    }
}
