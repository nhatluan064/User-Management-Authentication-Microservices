namespace AuthenticationService.Models;

public class Delegation
{
    public int Id { get; set; }
    public int DelegatorId { get; set; } // Người ủy quyền
    public int DelegateeId { get; set; } // Người được ủy quyền
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? PdfPath { get; set; } // Path to exported PDF
    public bool EmailSent { get; set; } = false;

    // Navigation properties
    public User Delegator { get; set; } = null!;
    public User Delegatee { get; set; } = null!;
}

