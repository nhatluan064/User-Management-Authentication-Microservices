using AuthenticationService.Models;
using AuthenticationService.Models.DTOs;
using AuthenticationService.Repositories;

namespace AuthenticationService.Services;

public class DelegationService : IDelegationService
{
    private readonly IDelegationRepository _delegationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPdfService _pdfService;
    private readonly IEmailService _emailService;
    private readonly ILogger<DelegationService> _logger;

    public DelegationService(
        IDelegationRepository delegationRepository,
        IUserRepository userRepository,
        IPdfService pdfService,
        IEmailService emailService,
        ILogger<DelegationService> logger)
    {
        _delegationRepository = delegationRepository;
        _userRepository = userRepository;
        _pdfService = pdfService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<DelegationInfo> CreateDelegationAsync(int delegatorId, CreateDelegationRequest request)
    {
        // Validate dates
        if (request.StartDate >= request.EndDate)
        {
            throw new InvalidOperationException("Start date must be before end date");
        }

        if (request.StartDate < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Start date cannot be in the past");
        }

        // Check if delegator exists
        var delegator = await _userRepository.GetByIdAsync(delegatorId);
        if (delegator == null)
        {
            throw new InvalidOperationException("Delegator not found");
        }

        // Check if delegatee exists
        var delegatee = await _userRepository.GetByIdAsync(request.DelegateeId);
        if (delegatee == null)
        {
            throw new InvalidOperationException("Delegatee not found");
        }

        // Check for overlapping delegations
        var existingDelegations = await _delegationRepository.GetActiveDelegationsForDelegateeAsync(request.DelegateeId);
        var hasOverlap = existingDelegations.Any(d => 
            (d.StartDate <= request.StartDate && d.EndDate >= request.StartDate) ||
            (d.StartDate <= request.EndDate && d.EndDate >= request.EndDate) ||
            (d.StartDate >= request.StartDate && d.EndDate <= request.EndDate));

        if (hasOverlap)
        {
            throw new InvalidOperationException("Delegation period overlaps with existing delegation");
        }

        var delegation = new Delegation
        {
            DelegatorId = delegatorId,
            DelegateeId = request.DelegateeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            IsActive = true
        };

        delegation = await _delegationRepository.CreateAsync(delegation);

        // Generate PDF
        try
        {
            var pdfBytes = await _pdfService.GenerateDelegationPdfAsync(delegation);
            if (pdfBytes != null)
            {
                // Save PDF to file system (in production, use cloud storage)
                var pdfFileName = $"delegation_{delegation.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                var pdfPath = Path.Combine("wwwroot", "delegations", pdfFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(pdfPath)!);
                await File.WriteAllBytesAsync(pdfPath, pdfBytes);
                delegation.PdfPath = pdfPath;
                await _delegationRepository.UpdateAsync(delegation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for delegation {DelegationId}", delegation.Id);
        }

        // Send email
        try
        {
            if (delegatee.Email != null)
            {
                await _emailService.SendDelegationEmailAsync(delegation, delegator, delegatee);
                delegation.EmailSent = true;
                await _delegationRepository.UpdateAsync(delegation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email for delegation {DelegationId}", delegation.Id);
        }

        // Reload with navigation properties
        var reloadedDelegation = await _delegationRepository.GetByIdAsync(delegation.Id);
        if (reloadedDelegation == null) throw new InvalidOperationException("Failed to create delegation");

        return new DelegationInfo
        {
            DelegationId = reloadedDelegation.Id,
            Delegator = MapToUserInfo(delegator),
            StartDate = reloadedDelegation.StartDate,
            EndDate = reloadedDelegation.EndDate,
            Reason = reloadedDelegation.Reason
        };
    }

    public async Task<List<DelegationInfo>> GetActiveDelegationsForUserAsync(int userId)
    {
        var delegations = await _delegationRepository.GetActiveDelegationsForDelegateeAsync(userId);
        var result = new List<DelegationInfo>();

        foreach (var delegation in delegations)
        {
            var delegator = await _userRepository.GetByIdAsync(delegation.DelegatorId);
            if (delegator != null)
            {
                result.Add(new DelegationInfo
                {
                    DelegationId = delegation.Id,
                    Delegator = MapToUserInfo(delegator),
                    StartDate = delegation.StartDate,
                    EndDate = delegation.EndDate,
                    Reason = delegation.Reason
                });
            }
        }

        return result;
    }

    public async Task<bool> CancelDelegationAsync(int delegationId, int userId)
    {
        var delegation = await _delegationRepository.GetByIdAsync(delegationId);
        if (delegation == null) return false;

        // Only delegator or delegatee can cancel
        if (delegation.DelegatorId != userId && delegation.DelegateeId != userId)
        {
            return false;
        }

        delegation.IsActive = false;
        await _delegationRepository.UpdateAsync(delegation);
        return true;
    }

    public async Task<byte[]?> GenerateDelegationPdfAsync(int delegationId)
    {
        var delegation = await _delegationRepository.GetByIdAsync(delegationId);
        if (delegation == null) return null;

        return await _pdfService.GenerateDelegationPdfAsync(delegation);
    }

    private UserInfo MapToUserInfo(User user)
    {
        return new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            IsLocalUser = user.IsLocalUser,
            Departments = user.UserDepartments.Select(ud => new DepartmentInfo
            {
                Id = ud.Department.Id,
                Name = ud.Department.Name,
                Code = ud.Department.Code,
                Role = ud.Role != null ? new RoleInfo
                {
                    Id = ud.Role.Id,
                    Name = ud.Role.Name
                } : null
            }).ToList()
        };
    }
}

