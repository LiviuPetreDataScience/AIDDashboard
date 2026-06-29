using AidDashboard.Application.Tabs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>SLAs &amp; KPIs tab.</summary>
[ApiController]
[Route("api/accounts/{accountId:int}/sla-kpis")]
[Authorize]
public class SlaKpisController : ControllerBase
{
    private readonly ISlaKpiService _slaKpiService;

    public SlaKpisController(ISlaKpiService slaKpiService)
    {
        _slaKpiService = slaKpiService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SlaKpiDto>>> Get(int accountId, CancellationToken cancellationToken)
    {
        var rows = await _slaKpiService.GetAsync(accountId, cancellationToken);
        return Ok(rows);
    }

    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Save(
        int accountId,
        [FromBody] IReadOnlyList<SlaKpiDto> rows,
        CancellationToken cancellationToken)
    {
        var saved = await _slaKpiService.SaveAsync(accountId, rows, cancellationToken);
        return saved ? NoContent() : NotFound();
    }
}
