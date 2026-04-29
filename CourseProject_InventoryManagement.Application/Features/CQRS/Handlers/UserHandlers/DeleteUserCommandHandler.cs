using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.UserHandlers
{
    public class DeleteUserCommandHandler : ICQRS.IDeleteUser
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public DeleteUserCommandHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Result<Guid>> DeleteUser(DeleteUserCommand command, CancellationToken cancellationToken = default)
        {
            var adminResult = await _authenticatedUserService.GetRequiredAdminAsync(cancellationToken);
            if (!adminResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(adminResult);
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == command.UserId && !x.IsDeleted, cancellationToken);
            if (user is null)
            {
                return Result<Guid>.NotFound("User not found.");
            }

            user.IsDeleted = true;
            user.IsBlocked = true;
            user.DeletedAtUtc = DateTime.UtcNow;
            user.UpdatedByUserId = adminResult.Value.Id;

            var existingAccesses = await _context.InventoryAccesses
                .Where(x => x.UserId == user.Id)
                .ToListAsync(cancellationToken);

            if (existingAccesses.Count > 0)
            {
                _context.InventoryAccesses.RemoveRange(existingAccesses);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(user.Id);
        }
    }
}
