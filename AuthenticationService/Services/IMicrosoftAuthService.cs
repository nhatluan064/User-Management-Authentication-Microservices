namespace AuthenticationService.Services;

public interface IMicrosoftAuthService
{
    Task<bool> ValidateAdCredentialsAsync(string username, string password, string? domain = null);
    Task<MicrosoftUserInfo?> GetUserFromAdAsync(string username, string? domain = null);
    Task<MicrosoftUserInfo?> GetUserFromMicrosoft365Async(string entityId);
}

public class MicrosoftUserInfo
{
    public string ObjectId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? DisplayName { get; set; }
}

