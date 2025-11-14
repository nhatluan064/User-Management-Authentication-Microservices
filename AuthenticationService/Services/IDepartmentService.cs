using AuthenticationService.Models;

namespace AuthenticationService.Services;

public interface IDepartmentService
{
    Task<Department> CreateDepartmentAsync(string name, string? code = null, string? description = null);
    Task<List<Department>> GetAllDepartmentsAsync();
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<Department> UpdateDepartmentAsync(int id, string? name, string? code, string? description);
    Task<bool> DeleteDepartmentAsync(int id);
    Task<Role> CreateRoleAsync(int departmentId, string name, string? description = null);
    Task<List<Role>> GetRolesByDepartmentAsync(int departmentId);
}

