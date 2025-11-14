namespace AuthenticationService.Models.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserInfo UserInfo { get; set; } = null!;
    public DelegationInfo? DelegationInfo { get; set; } // Thông tin ủy quyền nếu có
}

