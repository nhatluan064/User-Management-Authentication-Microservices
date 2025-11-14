using AuthenticationService.Models;

namespace AuthenticationService.Repositories;

public interface IDelegationRepository
{
    Task<Delegation?> GetByIdAsync(int id);
    Task<List<Delegation>> GetActiveDelegationsForDelegateeAsync(int delegateeId);
    Task<List<Delegation>> GetActiveDelegationsForDelegatorAsync(int delegatorId);
    Task<Delegation> CreateAsync(Delegation delegation);
    Task<Delegation> UpdateAsync(Delegation delegation);
    Task<bool> DeleteAsync(int id);
    Task<List<Delegation>> GetAllAsync();
}

