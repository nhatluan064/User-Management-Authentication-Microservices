using Microsoft.EntityFrameworkCore;
using AuthenticationService.Data;
using AuthenticationService.Models;

namespace AuthenticationService.Repositories;

public class DelegationRepository : IDelegationRepository
{
    private readonly AppDbContext _context;

    public DelegationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Delegation?> GetByIdAsync(int id)
    {
        return await _context.Delegations
            .Include(d => d.Delegator)
            .Include(d => d.Delegatee)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<Delegation>> GetActiveDelegationsForDelegateeAsync(int delegateeId)
    {
        var now = DateTime.UtcNow;
        return await _context.Delegations
            .Where(d => d.DelegateeId == delegateeId 
                     && d.IsActive 
                     && d.StartDate <= now 
                     && d.EndDate >= now)
            .Include(d => d.Delegator)
            .Include(d => d.Delegatee)
            .ToListAsync();
    }

    public async Task<List<Delegation>> GetActiveDelegationsForDelegatorAsync(int delegatorId)
    {
        var now = DateTime.UtcNow;
        return await _context.Delegations
            .Where(d => d.DelegatorId == delegatorId 
                     && d.IsActive 
                     && d.StartDate <= now 
                     && d.EndDate >= now)
            .Include(d => d.Delegator)
            .Include(d => d.Delegatee)
            .ToListAsync();
    }

    public async Task<Delegation> CreateAsync(Delegation delegation)
    {
        _context.Delegations.Add(delegation);
        await _context.SaveChangesAsync();
        return delegation;
    }

    public async Task<Delegation> UpdateAsync(Delegation delegation)
    {
        _context.Delegations.Update(delegation);
        await _context.SaveChangesAsync();
        return delegation;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var delegation = await _context.Delegations.FindAsync(id);
        if (delegation == null) return false;

        _context.Delegations.Remove(delegation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Delegation>> GetAllAsync()
    {
        return await _context.Delegations
            .Include(d => d.Delegator)
            .Include(d => d.Delegatee)
            .ToListAsync();
    }
}

