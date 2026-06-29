using AidDashboard.Application.Tabs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>Contractual Languages tab (Location x Language matrix).</summary>
[ApiController]
[Route("api/accounts/{accountId:int}/contractual-languages")]
[Authorize]
public class ContractualLanguagesController : ControllerBase
{
    private readonly IContractualLanguageService _contractualLanguageService;

    public ContractualLanguagesController(IContractualLanguageService contractualLanguageService)
    {
        _contractualLanguageService = contractualLanguageService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContractualLanguageCellDto>>> Get(int accountId, CancellationToken cancellationToken)
    {
        var cells = await _contractualLanguageService.GetAsync(accountId, cancellationToken);
        return Ok(cells);
    }

    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Save(
        int accountId,
        [FromBody] IReadOnlyList<ContractualLanguageCellDto> cells,
        CancellationToken cancellationToken)
    {
        var saved = await _contractualLanguageService.SaveAsync(accountId, cells, cancellationToken);
        return saved ? NoContent() : NotFound();
    }
}
