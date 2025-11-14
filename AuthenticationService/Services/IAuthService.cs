using AuthenticationService.Models.DTOs;

namespace AuthenticationService.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<UserInfo?> ValidateTokenAsync(string token);
    Task<UserInfo?> GetUserInfoAsync(int userId);
    string GenerateToken(UserInfo userInfo, DelegationInfo? delegationInfo = null);
}

