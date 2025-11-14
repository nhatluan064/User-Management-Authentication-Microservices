using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthenticationService.Models.DTOs;
using AuthenticationService.Services;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // Tạm thời comment để tạo user đầu tiên
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("local")]
    public async Task<ActionResult<UserInfo>> CreateLocalUser([FromBody] CreateLocalUserRequest request)
    {
        try
        {
            var user = await _userService.CreateLocalUserAsync(request);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating local user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<UserInfo>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserInfo>> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var success = await _userService.UpdateUserAsync(id, request.Email, request.FullName);
            if (!success)
            {
                return NotFound();
            }
            return Ok(new { message = "User updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost("{id}/departments/{departmentId}")]
    public async Task<ActionResult<UserInfo>> AssignUserToDepartment(
        int id, 
        int departmentId, 
        [FromQuery] int? roleId = null)
    {
        try
        {
            var user = await _userService.AssignUserToDepartmentAsync(id, departmentId, roleId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user to department");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
}

