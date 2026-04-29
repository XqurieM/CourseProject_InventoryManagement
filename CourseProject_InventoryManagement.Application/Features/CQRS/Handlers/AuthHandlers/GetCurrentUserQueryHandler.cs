using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.AuthQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.AuthHandlers
{
    public class GetCurrentUserQueryHandler : ICQRS.IGetCurrentUser
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetCurrentUserQueryHandler(IAuthenticatedUserService authenticatedUserService)
        {
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Result<UserDto>> GetCurrentUser(GetCurrentUserQuery query, CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);

            if (!userResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<CourseProject_InventoryManagement.Domain.Entities.AppUser, UserDto>(userResult);
            }

            var user = userResult.Value;

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
