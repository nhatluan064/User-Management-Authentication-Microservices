using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthenticationService.Models;
using AuthenticationService.Services;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Department>> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        try
        {
            var department = await _departmentService.CreateDepartmentAsync(
                request.Name, 
                request.Code, 
                request.Description);
            return Ok(department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<Department>>> GetAllDepartments()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Department>> GetDepartmentById(int id)
    {
        try
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return Ok(department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Department>> UpdateDepartment(int id, [FromBody] UpdateDepartmentRequest request)
    {
        try
        {
            var department = await _departmentService.UpdateDepartmentAsync(
                id, 
                request.Name, 
                request.Code, 
                request.Description);
            return Ok(department);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDepartment(int id)
    {
        try
        {
            var success = await _departmentService.DeleteDepartmentAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return Ok(new { message = "Department deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost("{id}/roles")]
    public async Task<ActionResult<Role>> CreateRole(int id, [FromBody] CreateRoleRequest request)
    {
        try
        {
            var role = await _departmentService.CreateRoleAsync(id, request.Name, request.Description);
            return Ok(role);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("{id}/roles")]
    public async Task<ActionResult<List<Role>>> GetRolesByDepartment(int id)
    {
        try
        {
            var roles = await _departmentService.GetRolesByDepartmentAsync(id);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for department {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}

public class CreateDepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
}

public class UpdateDepartmentRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
}

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

