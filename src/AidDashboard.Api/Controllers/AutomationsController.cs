using AidDashboard.Application.Tabs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>Automations tab.</summary>
[ApiController]
[Route("api/accounts/{accountId:int}/automations")]
[Authorize]
public class AutomationsController : ControllerBase
{
    private readonly IAutomationService _automationService;

    public AutomationsController(IAutomationService automationService)
    {
        _automationService = automationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AutomationDto>>> Get(int accountId, CancellationToken cancellationToken)
    {
        var rows = await _automationService.GetAsync(accountId, cancellationToken);
        return Ok(rows);
    }

    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Save(
        int accountId,
        [FromBody] IReadOnlyList<AutomationDto> rows,
        CancellationToken cancellationToken)
    {
        var saved = await _automationService.SaveAsync(accountId, rows, cancellationToken);
        return saved ? NoContent() : NotFound();
    }
}
