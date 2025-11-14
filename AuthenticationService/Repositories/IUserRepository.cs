using AuthenticationService.Models;

namespace AuthenticationService.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByAdObjectIdAsync(string adObjectId);
    Task<User?> GetByMicrosoft365IdAsync(string microsoft365Id);
    Task<List<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<List<User>> GetUsersByDepartmentAsync(int departmentId);
}

