using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>
/// Initial Staffing Model and Latest Approved Model tabs. The model is selected by the
/// route segment (Initial or LatestApproved).
/// </summary>
[ApiController]
[Route("api/accounts/{accountId:int}/staffing/{modelType}")]
[Authorize]
public class StaffingController : ControllerBase
{
    private readonly IStaffingService _staffingService;

    public StaffingController(IStaffingService staffingService)
    {
        _staffingService = staffingService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffingCellDto>>> Get(
        int accountId,
        StaffingModelType modelType,
        CancellationToken cancellationToken)
    {
        var cells = await _staffingService.GetAsync(accountId, modelType, cancellationToken);
        return Ok(cells);
    }

    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Save(
        int accountId,
        StaffingModelType modelType,
        [FromBody] IReadOnlyList<StaffingCellDto> cells,
        CancellationToken cancellationToken)
    {
        var saved = await _staffingService.SaveAsync(accountId, modelType, cells, cancellationToken);
        return saved ? NoContent() : NotFound();
    }
}
