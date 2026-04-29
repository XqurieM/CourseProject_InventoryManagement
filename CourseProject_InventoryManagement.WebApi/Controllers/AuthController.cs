using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.AuthQueries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRegisterUser _registerUser;
        private readonly ILoginUser _loginUser;
        private readonly IRefreshAccessToken _refreshAccessToken;
        private readonly IRevokeRefreshToken _revokeRefreshToken;
        private readonly IGetCurrentUser _getCurrentUser;
        private readonly IUpdateUserLanguage _updateUserLanguage;
        private readonly IUpdateUserTheme _updateUserTheme;

        public AuthController(
            IRegisterUser registerUser,
            ILoginUser loginUser,
            IRefreshAccessToken refreshAccessToken,
            IRevokeRefreshToken revokeRefreshToken,
            IGetCurrentUser getCurrentUser,
            IUpdateUserLanguage updateUserLanguage,
            IUpdateUserTheme updateUserTheme)
        {
            _registerUser = registerUser;
            _loginUser = loginUser;
            _refreshAccessToken = refreshAccessToken;
            _revokeRefreshToken = revokeRefreshToken;
            _getCurrentUser = getCurrentUser;
            _updateUserLanguage = updateUserLanguage;
            _updateUserTheme = updateUserTheme;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<AuthTokenDto>> Register(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _registerUser.RegisterUser(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<AuthTokenDto>> Login(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _loginUser.LoginUser(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<AuthTokenDto>> Refresh(RefreshAccessTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await _refreshAccessToken.RefreshAccessToken(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
        {
            var result = await _getCurrentUser.GetCurrentUser(new GetCurrentUserQuery(), cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UserDto>> UpdateLanguage(UpdateUserLanguageCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateUserLanguage.UpdateUserLanguage(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UserDto>> UpdateTheme(UpdateUserThemeCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateUserTheme.UpdateUserTheme(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> RevokeRefreshToken(RevokeRefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await _revokeRefreshToken.RevokeRefreshToken(command, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
