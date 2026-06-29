using AidDashboard.Application.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>
/// Manages accounts and their "Overall" tab. Reads are open to any authenticated user;
/// creating, editing and deleting accounts require an administrator.
/// </summary>
[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>Returns all accounts for the account picker.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountSummaryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var accounts = await _accountService.GetAllAsync(cancellationToken);
        return Ok(accounts);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<AccountSummaryDto>> Create(
        [FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _accountService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOverall), new { id = created.Id }, created);
    }

    /// <summary>Returns the "Overall" tab data for an account.</summary>
    [HttpGet("{id:int}/overall")]
    public async Task<ActionResult<AccountOverallDto>> GetOverall(int id, CancellationToken cancellationToken)
    {
        var overall = await _accountService.GetOverallAsync(id, cancellationToken);
        return overall is null ? NotFound() : Ok(overall);
    }

    [HttpPut("{id:int}/overall")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateOverall(
        int id,
        [FromBody] AccountOverallDto overall,
        CancellationToken cancellationToken)
    {
        var updated = await _accountService.UpdateOverallAsync(id, overall, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _accountService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
