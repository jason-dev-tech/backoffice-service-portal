using Microsoft.AspNetCore.Identity;

namespace BackofficeServicePortal.Api.Services;

public class PasswordHasherService : BackofficeServicePortal.Api.Services.Interfaces.IPasswordHasherService
{
    private readonly PasswordHasher<BackofficeServicePortal.Api.Models.User> _passwordHasher = new();

    public string HashPassword(BackofficeServicePortal.Api.Models.User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(BackofficeServicePortal.Api.Models.User user, string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
