using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using InvoiceManagement.Models.DTOs;
using InvoiceManagement.Models.Entities;
using InvoiceManagement.Services;

namespace InvoiceManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<User> userManager, IJwtTokenService jwtTokenService, IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !user.IsActive)
        {
            return Unauthorized(new { error = "Invalid username or password." });
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return Unauthorized(new { error = "Invalid username or password." });
        }

        // Generate JWT token
        var token = _jwtTokenService.GenerateToken(user);
        var expiryMinutes = Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"] ?? "30");

        return Ok(new LoginResponse
        {
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = user.Role.ToString(),
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Message = "Login successful"
        });
    }

    /// <summary>
    /// Logout current user (with JWT, logout is client-side - this endpoint is for compatibility)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        // With JWT tokens, logout is typically handled client-side by deleting the token
        // This endpoint is provided for consistency but doesn't need to do anything server-side
        return Ok(new { message = "Logout successful. Please delete your token on the client side." });
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { error = "User not found." });
        }

        return Ok(new LoginResponse
        {
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = user.Role.ToString(),
            Message = "User info retrieved"
        });
    }
}
