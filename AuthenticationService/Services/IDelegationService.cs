using AuthenticationService.Models;
using AuthenticationService.Models.DTOs;

namespace AuthenticationService.Services;

public interface IDelegationService
{
    Task<DelegationInfo> CreateDelegationAsync(int delegatorId, CreateDelegationRequest request);
    Task<List<DelegationInfo>> GetActiveDelegationsForUserAsync(int userId);
    Task<bool> CancelDelegationAsync(int delegationId, int userId);
    Task<byte[]?> GenerateDelegationPdfAsync(int delegationId);
}

