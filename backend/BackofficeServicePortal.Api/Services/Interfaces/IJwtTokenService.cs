namespace BackofficeServicePortal.Api.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(BackofficeServicePortal.Api.Models.User user, IEnumerable<string> roles, DateTime expiresAtUtc);
}
