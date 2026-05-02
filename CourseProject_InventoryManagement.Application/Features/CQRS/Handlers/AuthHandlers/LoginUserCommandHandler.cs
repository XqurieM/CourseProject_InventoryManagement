using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.AuthHandlers
{
    public class LoginUserCommandHandler : ICQRS.ILoginUser
    {
        private readonly IAppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public LoginUserCommandHandler(
            IAppDbContext context,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<Result<AuthTokenDto>> LoginUser(LoginUserCommand command, CancellationToken cancellationToken = default,bool isExternalLogin = false)
        {
            if (string.IsNullOrWhiteSpace(command.EmailOrUserName) || string.IsNullOrWhiteSpace(command.Password))
            {
                return Result<AuthTokenDto>.Invalid(new ValidationError("Credentials are required."));
            }

            var normalizedValue = command.EmailOrUserName.Trim().ToUpperInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    (x.NormalizedEmail == normalizedValue || x.NormalizedUserName == normalizedValue),
                    cancellationToken);

            if (user is null || (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash) && !isExternalLogin))
            {
                return Result<AuthTokenDto>.Unauthorized("Invalid credentials.");
            }

            if (user.IsBlocked)
            {
                return Result<AuthTokenDto>.Forbidden("Your account is blocked.");
            }

            var authToken = _jwtTokenService.CreateToken(user);
            var refreshToken = _refreshTokenService.CreateRefreshToken(user, authToken.RefreshToken);
            await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<AuthTokenDto>.Success(authToken);
        }
    }
}
