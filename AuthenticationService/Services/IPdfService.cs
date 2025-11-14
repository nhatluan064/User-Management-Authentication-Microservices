using AuthenticationService.Models;

namespace AuthenticationService.Services;

public interface IPdfService
{
    Task<byte[]?> GenerateDelegationPdfAsync(Delegation delegation);
}

