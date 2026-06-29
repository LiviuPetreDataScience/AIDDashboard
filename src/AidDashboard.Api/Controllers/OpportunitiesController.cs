using AidDashboard.Application.Tabs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>Opportunities tab.</summary>
[ApiController]
[Route("api/accounts/{accountId:int}/opportunities")]
[Authorize]
public class OpportunitiesController : ControllerBase
{
    private readonly IOpportunityService _opportunityService;

    public OpportunitiesController(IOpportunityService opportunityService)
    {
        _opportunityService = opportunityService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OpportunityDto>>> Get(int accountId, CancellationToken cancellationToken)
    {
        var rows = await _opportunityService.GetAsync(accountId, cancellationToken);
        return Ok(rows);
    }

    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Save(
        int accountId,
        [FromBody] IReadOnlyList<OpportunityDto> rows,
        CancellationToken cancellationToken)
    {
        var saved = await _opportunityService.SaveAsync(accountId, rows, cancellationToken);
        return saved ? NoContent() : NotFound();
    }
}
