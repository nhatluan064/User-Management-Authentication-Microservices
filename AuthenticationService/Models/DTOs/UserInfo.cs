namespace AuthenticationService.Models.DTOs;

public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool IsLocalUser { get; set; }
    public List<DepartmentInfo> Departments { get; set; } = new();
}

public class DepartmentInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public RoleInfo? Role { get; set; }
}

public class RoleInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

