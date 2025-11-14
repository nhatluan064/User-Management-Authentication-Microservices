namespace AuthenticationService.Models.DTOs;

public class CreateDelegationRequest
{
    public int DelegateeId { get; set; } // Người được ủy quyền
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}

