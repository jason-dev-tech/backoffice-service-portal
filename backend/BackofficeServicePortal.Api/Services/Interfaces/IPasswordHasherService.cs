namespace BackofficeServicePortal.Api.Services.Interfaces;

public interface IPasswordHasherService
{
    string HashPassword(BackofficeServicePortal.Api.Models.User user, string password);

    bool VerifyPassword(BackofficeServicePortal.Api.Models.User user, string hashedPassword, string providedPassword);
}
