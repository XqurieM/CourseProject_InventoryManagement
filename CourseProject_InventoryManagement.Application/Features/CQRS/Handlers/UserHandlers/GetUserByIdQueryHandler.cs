using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.UserQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.UserHandlers
{
    public class GetUserByIdQueryHandler : ICQRS.IGetUsersById
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IAdministrationAuthorizationService _administrationAuthorizationService;

        public GetUserByIdQueryHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IAdministrationAuthorizationService administrationAuthorizationService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _administrationAuthorizationService = administrationAuthorizationService;
        }

        public async Task<Result<UserDto>> GetUsersById(GetUserByIdQuery query, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, UserDto>(userResult);
            }

            var canManageUsers = await _administrationAuthorizationService.CanManageUsersAsync(userResult.Value.Id, cancellationToken);
            if (!canManageUsers)
            {
                return Result<UserDto>.Forbidden("You do not have permission to view this user.");
            }

            var userControl = _context.Users.Where(u => u.Id == query.Id).FirstOrDefault();
            if (userControl == null)
            {
                return Result<UserDto>.NotFound();
            }

            var userDto = new UserDto
            {
                Id = userControl.Id,
                UserName = userControl.UserName,
                Email = userControl.Email,
                IsAdmin = userControl.IsAdmin,
                IsBlocked = userControl.IsBlocked,
                PreferredLanguage = userControl.PreferredLanguage,
                PreferredTheme = userControl.PreferredTheme
            };

            return Result<UserDto>.Success(userDto);
        }
    }
}
