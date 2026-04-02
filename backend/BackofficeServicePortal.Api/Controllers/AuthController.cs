using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackofficeServicePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BackofficeServicePortal.Api.Services.Interfaces.IAuthService _authService;

    public AuthController(BackofficeServicePortal.Api.Services.Interfaces.IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<BackofficeServicePortal.Api.DTOs.Auth.LoginResponseDto>> Login(BackofficeServicePortal.Api.DTOs.Auth.LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (result is null)
        {
            return Unauthorized();
        }

        return Ok(result);
    }
}
