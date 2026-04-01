namespace BackofficeServicePortal.Api.Services.Interfaces;

public interface IAuthService
{
    Task<BackofficeServicePortal.Api.DTOs.Auth.LoginResponseDto?> LoginAsync(BackofficeServicePortal.Api.DTOs.Auth.LoginRequestDto request);
}
