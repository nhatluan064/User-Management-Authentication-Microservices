using AuthenticationService.Models;
using AuthenticationService.Models.DTOs;
using AuthenticationService.Repositories;

namespace AuthenticationService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IDepartmentRepository departmentRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    public async Task<UserInfo?> CreateLocalUserAsync(CreateLocalUserRequest request)
    {
        // Check if username already exists
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        if (!string.IsNullOrEmpty(request.Email))
        {
            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException("Email already exists");
            }
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = passwordHash,
            IsLocalUser = true,
            IsActive = true
        };

        user = await _userRepository.CreateAsync(user);
        var createdUser = await _userRepository.GetByIdAsync(user.Id);
        if (createdUser == null) throw new InvalidOperationException("Failed to create user");

        return MapToUserInfo(createdUser);
    }

    public async Task<UserInfo?> AssignUserToDepartmentAsync(int userId, int departmentId, int? roleId = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department == null) return null;

        // Check if already assigned
        var existingAssignment = user.UserDepartments.FirstOrDefault(ud => ud.DepartmentId == departmentId);
        if (existingAssignment != null)
        {
            // Update role if provided
            if (roleId.HasValue)
            {
                existingAssignment.RoleId = roleId;
                user = await _userRepository.UpdateAsync(user);
            }
        }
        else
        {
            // Create new assignment
            user.UserDepartments.Add(new UserDepartment
            {
                UserId = userId,
                DepartmentId = departmentId,
                RoleId = roleId
            });
            user = await _userRepository.UpdateAsync(user);
        }

        var updatedUser = await _userRepository.GetByIdAsync(userId);
        if (updatedUser == null) return null;
        return MapToUserInfo(updatedUser);
    }

    public async Task<List<UserInfo>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToUserInfo).ToList();
    }

    public async Task<UserInfo?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;
        return MapToUserInfo(user);
    }

    public async Task<bool> UpdateUserAsync(int id, string? email, string? fullName)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        if (!string.IsNullOrEmpty(email) && email != user.Email)
        {
            var existingEmail = await _userRepository.GetByEmailAsync(email);
            if (existingEmail != null && existingEmail.Id != id)
            {
                throw new InvalidOperationException("Email already exists");
            }
            user.Email = email;
        }

        if (!string.IsNullOrEmpty(fullName))
        {
            user.FullName = fullName;
        }

        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    private UserInfo MapToUserInfo(User user)
    {
        return new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            IsLocalUser = user.IsLocalUser,
            Departments = user.UserDepartments.Select(ud => new DepartmentInfo
            {
                Id = ud.Department.Id,
                Name = ud.Department.Name,
                Code = ud.Department.Code,
                Role = ud.Role != null ? new RoleInfo
                {
                    Id = ud.Role.Id,
                    Name = ud.Role.Name
                } : null
            }).ToList()
        };
    }
}

