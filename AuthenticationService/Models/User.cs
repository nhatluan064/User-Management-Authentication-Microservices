namespace AuthenticationService.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? PasswordHash { get; set; } // For local users
    public bool IsLocalUser { get; set; } // true if not from AD
    public string? AdDomain { get; set; } // AD domain if from AD
    public string? AdObjectId { get; set; } // AD object ID
    public string? Microsoft365Id { get; set; } // Microsoft 365 Entity ID
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<UserDepartment> UserDepartments { get; set; } = new List<UserDepartment>();
}

