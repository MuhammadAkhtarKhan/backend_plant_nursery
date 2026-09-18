using Microsoft.AspNetCore.Mvc;
using PlantNursery.Application.Common;
using PlantNursery.Application.DTOs.Auth;
using PlantNursery.Application.Interfaces;

namespace PlantNursery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);

        return Ok( ApiResponse<AuthResponse>.Ok(response, "Registration successful."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        return Ok( ApiResponse<AuthResponse>.Ok(response, "Login successful."));
    }
}