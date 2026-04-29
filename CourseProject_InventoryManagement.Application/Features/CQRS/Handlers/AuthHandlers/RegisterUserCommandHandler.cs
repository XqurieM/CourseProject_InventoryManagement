using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.AuthHandlers
{
    public class RegisterUserCommandHandler : ICQRS.IRegisterUser
    {
        private readonly IAppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public RegisterUserCommandHandler(
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

        public async Task<Result<AuthTokenDto>> RegisterUser(RegisterUserCommand command, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command.UserName))
            {
                return Result<AuthTokenDto>.Invalid(new ValidationError(nameof(command.UserName), "User name is required."));
            }

            if (string.IsNullOrWhiteSpace(command.Email))
            {
                return Result<AuthTokenDto>.Invalid(new ValidationError(nameof(command.Email), "Email is required."));
            }

            if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 6)
            {
                return Result<AuthTokenDto>.Invalid(new ValidationError(nameof(command.Password), "Password must be at least 6 characters."));
            }

            var normalizedUserName = command.UserName.Trim().ToUpperInvariant();
            var normalizedEmail = command.Email.Trim().ToUpperInvariant();

            var userNameExists = await _context.Users.AnyAsync(x => x.NormalizedUserName == normalizedUserName, cancellationToken);
            if (userNameExists)
            {
                return Result<AuthTokenDto>.Conflict("User name is already in use.");
            }

            var emailExists = await _context.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
            if (emailExists)
            {
                return Result<AuthTokenDto>.Conflict("Email is already in use.");
            }

            var hasUsers = await _context.Users.AnyAsync(x => !x.IsDeleted, cancellationToken);

            var user = new AppUser
            {
                UserName = command.UserName.Trim(),
                NormalizedUserName = normalizedUserName,
                Email = command.Email.Trim(),
                NormalizedEmail = normalizedEmail,
                PasswordHash = _passwordHasher.HashPassword(command.Password),
                IsAdmin = !hasUsers,
                CreatedByUserId = Guid.Empty
            };

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            user.CreatedByUserId = user.Id;
            await _context.SaveChangesAsync(cancellationToken);

            var authToken = _jwtTokenService.CreateToken(user);
            var refreshToken = _refreshTokenService.CreateRefreshToken(user, authToken.RefreshToken);
            await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<AuthTokenDto>.Success(authToken);
        }
    }
}
