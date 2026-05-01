using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.UserQueries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IGetUsers _getUsers;
        private readonly IBlockUser _blockUser;
        private readonly IUnblockUser _unblockUser;
        private readonly IDeleteUser _deleteUser;
        private readonly IGrantAdminRole _grantAdminRole;
        private readonly IRevokeAdminRole _revokeAdminRole;
        private readonly IGetUsersById _getUsersById;

        public AdminController(IGetUsers getUsers, IBlockUser blockUser, IUnblockUser unblockUser, IDeleteUser deleteUser, IGrantAdminRole grantAdminRole, IRevokeAdminRole revokeAdminRole, IGetUsersById getUsersById)
        {
            _getUsers = getUsers;
            _blockUser = blockUser;
            _unblockUser = unblockUser;
            _deleteUser = deleteUser;
            _grantAdminRole = grantAdminRole;
            _revokeAdminRole = revokeAdminRole;
            _getUsersById = getUsersById;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetUsers(CancellationToken cancellationToken)
        {
            var result = await _getUsers.GetUsers(new GetUsersQuery(), cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpGet]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _getUsersById.GetUsersById(new GetUserByIdQuery { Id = id }, cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> BlockUser(BlockUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _blockUser.BlockUser(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> UnblockUser(UnblockUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _unblockUser.UnblockUser(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> DeleteUser(DeleteUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _deleteUser.DeleteUser(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> GrantAdminRole(GrantAdminRoleCommand command, CancellationToken cancellationToken)
        {
            var result = await _grantAdminRole.GrantAdminRole(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> RevokeAdminRole(RevokeAdminRoleCommand command, CancellationToken cancellationToken)
        {
            var result = await _revokeAdminRole.RevokeAdminRole(command, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
