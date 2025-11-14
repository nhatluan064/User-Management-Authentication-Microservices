using AuthenticationService.Models;

namespace AuthenticationService.Services;

public interface IEmailService
{
    Task SendDelegationEmailAsync(Delegation delegation, User delegator, User delegatee);
}

