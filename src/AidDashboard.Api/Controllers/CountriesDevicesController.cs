using AidDashboard.Application.Tabs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>Countries / Users / Devices Supported tab.</summary>
[ApiController]
[Route("api/accounts/{accountId:int}/countries-devices")]
[Authorize]
public class CountriesDevicesController : ControllerBase
{
    private readonly ICountryDeviceService _countryDeviceService;

    public CountriesDevicesController(ICountryDeviceService countryDeviceService)
    {
        _countryDeviceService = countryDeviceService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CountryRowDto>>> Get(int accountId, CancellationToken cancellationToken)
    {
        var rows = await _countryDeviceService.GetAsync(accountId, cancellationToken);
        return Ok(rows);
    }

    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Save(
        int accountId,
        [FromBody] IReadOnlyList<CountryRowDto> rows,
        CancellationToken cancellationToken)
    {
        var saved = await _countryDeviceService.SaveAsync(accountId, rows, cancellationToken);
        return saved ? NoContent() : NotFound();
    }
}
