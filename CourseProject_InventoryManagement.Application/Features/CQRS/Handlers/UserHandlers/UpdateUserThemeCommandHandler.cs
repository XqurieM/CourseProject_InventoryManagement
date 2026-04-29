using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.UserHandlers
{
    public class UpdateUserThemeCommandHandler : ICQRS.IUpdateUserTheme
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public UpdateUserThemeCommandHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Result<UserDto>> UpdateUserTheme(UpdateUserThemeCommand command, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, UserDto>(userResult);
            }

            var user = userResult.Value;
            user.PreferredTheme = command.PreferredTheme;
            user.UpdatedByUserId = user.Id;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<UserDto>.Success(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsAdmin = user.IsAdmin,
                IsBlocked = user.IsBlocked,
                PreferredLanguage = user.PreferredLanguage,
                PreferredTheme = user.PreferredTheme
            });
        }
    }
}
