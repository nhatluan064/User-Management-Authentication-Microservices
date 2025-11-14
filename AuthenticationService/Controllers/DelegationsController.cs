using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthenticationService.Models.DTOs;
using AuthenticationService.Services;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DelegationsController : ControllerBase
{
    private readonly IDelegationService _delegationService;
    private readonly ILogger<DelegationsController> _logger;

    public DelegationsController(IDelegationService delegationService, ILogger<DelegationsController> logger)
    {
        _delegationService = delegationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<DelegationInfo>> CreateDelegation([FromBody] CreateDelegationRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var delegatorId))
            {
                return Unauthorized();
            }

            var delegation = await _delegationService.CreateDelegationAsync(delegatorId, request);
            return Ok(delegation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating delegation");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("my-delegations")]
    public async Task<ActionResult<List<DelegationInfo>>> GetMyDelegations()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var delegations = await _delegationService.GetActiveDelegationsForUserAsync(userId);
            return Ok(delegations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delegations");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelDelegation(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var success = await _delegationService.CancelDelegationAsync(id, userId);
            if (!success)
            {
                return NotFound(new { message = "Delegation not found or you don't have permission to cancel it" });
            }

            return Ok(new { message = "Delegation cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling delegation {DelegationId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("{id}/pdf")]
    public async Task<ActionResult> DownloadDelegationPdf(int id)
    {
        try
        {
            var pdfBytes = await _delegationService.GenerateDelegationPdfAsync(id);
            if (pdfBytes == null)
            {
                return NotFound();
            }

            return File(pdfBytes, "application/pdf", $"delegation_{id}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for delegation {DelegationId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}

