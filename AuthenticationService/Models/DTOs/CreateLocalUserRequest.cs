namespace AuthenticationService.Models.DTOs;

public class CreateLocalUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public List<int> DepartmentIds { get; set; } = new();
    public Dictionary<int, int?> DepartmentRoles { get; set; } = new(); // DepartmentId -> RoleId
}

