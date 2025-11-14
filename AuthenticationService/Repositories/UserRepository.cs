using Microsoft.EntityFrameworkCore;
using AuthenticationService.Data;
using AuthenticationService.Models;

namespace AuthenticationService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Department)
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Department)
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Role)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Department)
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByAdObjectIdAsync(string adObjectId)
    {
        return await _context.Users
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Department)
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Role)
            .FirstOrDefaultAsync(u => u.AdObjectId == adObjectId);
    }

    public async Task<User?> GetByMicrosoft365IdAsync(string microsoft365Id)
    {
        return await _context.Users
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Department)
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Role)
            .FirstOrDefaultAsync(u => u.Microsoft365Id == microsoft365Id);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Department)
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Role)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<User>> GetUsersByDepartmentAsync(int departmentId)
    {
        return await _context.Users
            .Where(u => u.UserDepartments.Any(ud => ud.DepartmentId == departmentId))
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Department)
            .Include(u => u.UserDepartments)
                .ThenInclude(ud => ud.Role)
            .ToListAsync();
    }
}

