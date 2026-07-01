using AidDashboard.Application.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>Read-only data feeds for the AID Dashboard views. Available to any authenticated user.</summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>All automation facts across accounts (the Automations dashboard filters them client-side).</summary>
    [HttpGet("automations")]
    public async Task<ActionResult<IReadOnlyList<AutomationFactDto>>> GetAutomationFacts(CancellationToken cancellationToken)
    {
        var facts = await _dashboardService.GetAutomationFactsAsync(cancellationToken);
        return Ok(facts);
    }

    /// <summary>The editable attribute profile (category / goal / environment / AI) per automation.</summary>
    [HttpGet("automation-profiles")]
    public async Task<ActionResult<IReadOnlyList<AutomationProfileDto>>> GetAutomationProfiles(CancellationToken cancellationToken)
    {
        var profiles = await _dashboardService.GetAutomationProfilesAsync(cancellationToken);
        return Ok(profiles);
    }

    /// <summary>Creates or updates the attribute profile for one automation (administrators only).</summary>
    [HttpPut("automation-profiles/{automationRefId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SaveAutomationProfile(
        int automationRefId,
        [FromBody] UpdateAutomationProfileRequest request,
        CancellationToken cancellationToken)
    {
        var saved = await _dashboardService.SaveAutomationProfileAsync(automationRefId, request, cancellationToken);
        return saved ? NoContent() : NotFound();
    }
}
