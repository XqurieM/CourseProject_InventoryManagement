using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.UserQueries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.UserHandlers
{
    public class GetUserByIdQueryHandler : ICQRS.IGetUsersById
    {
        IAppDbContext _context;

        public GetUserByIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<UserDto>> GetUsersById(GetUserByIdQuery query, CancellationToken cancellationToken = default)
        {
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
