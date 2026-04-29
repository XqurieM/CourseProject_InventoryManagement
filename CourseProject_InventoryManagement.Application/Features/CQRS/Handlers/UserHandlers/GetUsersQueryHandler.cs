using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.UserQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.UserHandlers
{
    public class GetUsersQueryHandler : ICQRS.IGetUsers
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetUsersQueryHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Result<List<UserDto>>> GetUsers(GetUsersQuery query, CancellationToken cancellationToken = default)
        {
            var adminResult = await _authenticatedUserService.GetRequiredAdminAsync(cancellationToken);
            if (!adminResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<CourseProject_InventoryManagement.Domain.Entities.AppUser, List<UserDto>>(adminResult);
            }

            var users = await _context.Users
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.UserName)
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    IsAdmin = x.IsAdmin,
                    IsBlocked = x.IsBlocked,
                    PreferredLanguage = x.PreferredLanguage,
                    PreferredTheme = x.PreferredTheme
                })
                .ToListAsync(cancellationToken);

            return Result<List<UserDto>>.Success(users);
        }
    }
}
