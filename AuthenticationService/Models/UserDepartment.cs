namespace AuthenticationService.Models;

public class UserDepartment
{
    public int UserId { get; set; }
    public int DepartmentId { get; set; }
    public int? RoleId { get; set; } // Role within this department
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public Role? Role { get; set; }
}

