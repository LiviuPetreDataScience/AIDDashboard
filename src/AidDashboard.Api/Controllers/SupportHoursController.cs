using AidDashboard.Application.Tabs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>Support Hours tab (one row per support-hours language).</summary>
[ApiController]
[Route("api/accounts/{accountId:int}/support-hours")]
[Authorize]
public class SupportHoursController : ControllerBase
{
    private readonly ISupportHourService _supportHourService;

    public SupportHoursController(ISupportHourService supportHourService)
    {
        _supportHourService = supportHourService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupportHourRowDto>>> Get(int accountId, CancellationToken cancellationToken)
    {
        var rows = await _supportHourService.GetAsync(accountId, cancellationToken);
        return Ok(rows);
    }

    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Save(
        int accountId,
        [FromBody] IReadOnlyList<SupportHourRowDto> rows,
        CancellationToken cancellationToken)
    {
        var saved = await _supportHourService.SaveAsync(accountId, rows, cancellationToken);
        return saved ? NoContent() : NotFound();
    }
}
