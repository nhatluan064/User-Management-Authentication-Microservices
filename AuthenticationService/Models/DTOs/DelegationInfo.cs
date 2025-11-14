namespace AuthenticationService.Models.DTOs;

public class DelegationInfo
{
    public int DelegationId { get; set; }
    public UserInfo Delegator { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}

