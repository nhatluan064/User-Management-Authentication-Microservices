using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthenticationService.Models.DTOs;
using AuthenticationService.Services;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<UserInfo>> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            var userInfo = await _authService.ValidateTokenAsync(request.Token);
            if (userInfo == null)
            {
                return Unauthorized(new { message = "Invalid or expired token" });
            }

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, new { message = "An error occurred during token validation" });
        }
    }

    [HttpGet("user-info")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> GetUserInfo()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var userInfo = await _authService.GetUserInfoAsync(userId);
            if (userInfo == null)
            {
                return NotFound();
            }

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // In a stateless JWT system, logout is typically handled client-side by removing the token
        // If you need server-side logout, implement token blacklisting
        return Ok(new { message = "Logged out successfully" });
    }
}

public class ValidateTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

