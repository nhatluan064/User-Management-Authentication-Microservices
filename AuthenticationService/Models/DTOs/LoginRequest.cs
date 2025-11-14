namespace AuthenticationService.Models.DTOs;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseLocalAuth { get; set; } = false; // true for local users, false for AD
}

