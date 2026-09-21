using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecamNewBackend.Common;
using RecamNewBackend.DTOs.Auth;
using RecamNewBackend.Exceptions;
using RecamNewBackend.Services;
using System.Security.Claims;

namespace RecamNewBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new photography company
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<string>>> Register([FromBody] RegisterDto dto)
    {
        var token = await _authService.RegisterAsync(dto);
        return Ok(ApiResponse<string>.Success(token, "Registration successful"));
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<string>>> Login([FromBody] LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);
        return Ok(ApiResponse<string>.Success(token, "Login successful"));
    }

    /// <summary>
    /// Update current user's password
    /// </summary>
    [Authorize]
    [HttpPatch("password")]
    public async Task<ActionResult<ApiResponse<string>>> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _authService.UpdatePasswordAsync(userId, dto);
        return Ok(ApiResponse<string>.Success("", "Password updated successfully"));
    }

    /// <summary>
    /// Get current logged-in user information
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<CurrentUserDto>>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var dto = await _authService.GetCurrentUserAsync(userId);
        return Ok(ApiResponse<CurrentUserDto>.Success(dto, "User retrieved successfully"));
    }
}
    
