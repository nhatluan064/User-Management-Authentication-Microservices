using AuthenticationService.Models;

namespace AuthenticationService.Repositories;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int id);
    Task<List<Department>> GetAllAsync();
    Task<Department> CreateAsync(Department department);
    Task<Department> UpdateAsync(Department department);
    Task<bool> DeleteAsync(int id);
}

