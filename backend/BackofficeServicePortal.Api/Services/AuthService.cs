using BackofficeServicePortal.Api.Configuration;
using BackofficeServicePortal.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BackofficeServicePortal.Api.Services;

public class AuthService : BackofficeServicePortal.Api.Services.Interfaces.IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly BackofficeServicePortal.Api.Services.Interfaces.IPasswordHasherService _passwordHasherService;
    private readonly BackofficeServicePortal.Api.Services.Interfaces.IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        AppDbContext dbContext,
        BackofficeServicePortal.Api.Services.Interfaces.IPasswordHasherService passwordHasherService,
        BackofficeServicePortal.Api.Services.Interfaces.IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<BackofficeServicePortal.Api.DTOs.Auth.LoginResponseDto?> LoginAsync(BackofficeServicePortal.Api.DTOs.Auth.LoginRequestDto request)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var isPasswordValid = _passwordHasherService.VerifyPassword(user, user.PasswordHash, request.Password);

        if (!isPasswordValid)
        {
            return null;
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);
        var accessToken = _jwtTokenService.GenerateToken(user, roles, expiresAtUtc);

        return new BackofficeServicePortal.Api.DTOs.Auth.LoginResponseDto
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc
        };
    }
}
