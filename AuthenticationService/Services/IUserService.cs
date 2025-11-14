using AuthenticationService.Models;
using AuthenticationService.Models.DTOs;

namespace AuthenticationService.Services;

public interface IUserService
{
    Task<UserInfo?> CreateLocalUserAsync(CreateLocalUserRequest request);
    Task<UserInfo?> AssignUserToDepartmentAsync(int userId, int departmentId, int? roleId = null);
    Task<List<UserInfo>> GetAllUsersAsync();
    Task<UserInfo?> GetUserByIdAsync(int id);
    Task<bool> UpdateUserAsync(int id, string? email, string? fullName);
    Task<bool> DeleteUserAsync(int id);
}

