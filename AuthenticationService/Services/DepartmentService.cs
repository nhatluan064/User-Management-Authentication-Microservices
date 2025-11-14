using Microsoft.EntityFrameworkCore;
using AuthenticationService.Data;
using AuthenticationService.Models;
using AuthenticationService.Repositories;

namespace AuthenticationService.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        AppDbContext context,
        ILogger<DepartmentService> logger)
    {
        _departmentRepository = departmentRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<Department> CreateDepartmentAsync(string name, string? code = null, string? description = null)
    {
        var department = new Department
        {
            Name = name,
            Code = code,
            Description = description,
            IsActive = true
        };

        return await _departmentRepository.CreateAsync(department);
    }

    public async Task<List<Department>> GetAllDepartmentsAsync()
    {
        return await _departmentRepository.GetAllAsync();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await _departmentRepository.GetByIdAsync(id);
    }

    public async Task<Department> UpdateDepartmentAsync(int id, string? name, string? code, string? description)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            throw new InvalidOperationException("Department not found");
        }

        if (!string.IsNullOrEmpty(name))
        {
            department.Name = name;
        }
        if (code != null)
        {
            department.Code = code;
        }
        if (description != null)
        {
            department.Description = description;
        }

        return await _departmentRepository.UpdateAsync(department);
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        return await _departmentRepository.DeleteAsync(id);
    }

    public async Task<Role> CreateRoleAsync(int departmentId, string name, string? description = null)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department == null)
        {
            throw new InvalidOperationException("Department not found");
        }

        var role = new Role
        {
            DepartmentId = departmentId,
            Name = name,
            Description = description,
            IsActive = true
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<List<Role>> GetRolesByDepartmentAsync(int departmentId)
    {
        return await _context.Roles
            .Where(r => r.DepartmentId == departmentId && r.IsActive)
            .ToListAsync();
    }
}

