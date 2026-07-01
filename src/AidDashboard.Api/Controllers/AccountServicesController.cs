using AidDashboard.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>Services tab data for an account (one row per service in the catalog).</summary>
[ApiController]
[Route("api/accounts/{accountId:int}/services")]
[Authorize]
public class AccountServicesController : ControllerBase
{
    private readonly IAccountServicesService _accountServices;

    public AccountServicesController(IAccountServicesService accountServices)
    {
        _accountServices = accountServices;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountServiceRowDto>>> Get(int accountId, CancellationToken cancellationToken)
    {
        var rows = await _accountServices.GetAsync(accountId, cancellationToken);
        return Ok(rows);
    }

    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Save(
        int accountId,
        [FromBody] IReadOnlyList<AccountServiceRowDto> rows,
        CancellationToken cancellationToken)
    {
        var saved = await _accountServices.SaveAsync(accountId, rows, cancellationToken);
        return saved ? NoContent() : NotFound();
    }
}
