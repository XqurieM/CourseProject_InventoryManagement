using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.AuthQueries;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CourseProject_InventoryManagement.WebApi.Options;
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
        private readonly IPasswordHasher _passwordHasher;
        private readonly MicrosoftExternalLoginOptions _microsoftExternalLoginOptions;

        public AuthController(IRegisterUser registerUser, ILoginUser loginUser, IRefreshAccessToken refreshAccessToken, IRevokeRefreshToken revokeRefreshToken, IGetCurrentUser getCurrentUser, IUpdateUserLanguage updateUserLanguage, IUpdateUserTheme updateUserTheme, IPasswordHasher passwordHasher, IOptions<MicrosoftExternalLoginOptions> microsoftExternalLoginOptions)
        {
            _registerUser = registerUser;
            _loginUser = loginUser;
            _refreshAccessToken = refreshAccessToken;
            _revokeRefreshToken = revokeRefreshToken;
            _getCurrentUser = getCurrentUser;
            _updateUserLanguage = updateUserLanguage;
            _updateUserTheme = updateUserTheme;
            _passwordHasher = passwordHasher;
            _microsoftExternalLoginOptions = microsoftExternalLoginOptions.Value;
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
        [HttpGet]
        public async Task<ActionResult<AuthTokenDto>> ExternalLoginGoogle(string Token)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { "422459194138-2pgs4h3tnbu9evi92amedpeg3vchjrk1.apps.googleusercontent.com" }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(Token, settings);

                var result = await HandleExternalLoginAsync(payload.Email, payload.Subject, HttpContext.RequestAborted);
                return this.ToActionResult(result);
            }
            catch (InvalidJwtException)
            {
                return BadRequest("Invalid Google Token.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred!");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<AuthTokenDto>> ExternalLoginMicrosoft(string Token)
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                return BadRequest("Microsoft token is required.");
            }

            if (string.IsNullOrWhiteSpace(_microsoftExternalLoginOptions.ClientId) || string.IsNullOrWhiteSpace(_microsoftExternalLoginOptions.TenantId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Microsoft login settings are missing.");
            }

            try
            {
                var principal = await ValidateMicrosoftTokenAsync(Token, HttpContext.RequestAborted);
                var email = principal.FindFirstValue(ClaimTypes.Email)
                    ?? principal.FindFirst("preferred_username")?.Value
                    ?? principal.FindFirst("upn")?.Value;
                var subject = principal.FindFirstValue("sub")
                    ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? email;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(subject))
                {
                    return BadRequest("Microsoft token does not contain the required identity claims.");
                }

                var result = await HandleExternalLoginAsync(email, subject, HttpContext.RequestAborted);
                return this.ToActionResult(result);
            }
            catch (SecurityTokenException)
            {
                return BadRequest("Invalid Microsoft token.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred!");
            }
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

        private async Task<Result<AuthTokenDto>> HandleExternalLoginAsync(string email, string providerSubject, CancellationToken cancellationToken)
        {
            var password = _passwordHasher.HashPassword(providerSubject);
            await _registerUser.RegisterUser(new RegisterUserCommand
            {
                Email = email,
                UserName = email.Split('@')[0],
                Password = password
            }, cancellationToken);

            return await _loginUser.LoginUser(new LoginUserCommand
            {
                Password = password,
                EmailOrUserName = email
            }, cancellationToken, true);
        }

        private async Task<ClaimsPrincipal> ValidateMicrosoftTokenAsync(string token, CancellationToken cancellationToken)
        {
            var tenantId = _microsoftExternalLoginOptions.TenantId;
            var metadataAddress = $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress,
                new OpenIdConnectConfigurationRetriever());

            var openIdConfiguration = await configurationManager.GetConfigurationAsync(cancellationToken);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    $"https://login.microsoftonline.com/{tenantId}/v2.0",
                    $"https://sts.windows.net/{tenantId}/"
                },
                ValidateAudience = true,
                ValidAudience = _microsoftExternalLoginOptions.ClientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = openIdConfiguration.SigningKeys,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
    }
}
