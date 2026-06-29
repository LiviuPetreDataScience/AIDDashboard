using AidDashboard.Application.Reference;
using AidDashboard.Domain.Reference;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>
/// Reads reference lists (available to any authenticated user) and lets administrators
/// maintain them. These lists populate the dropdowns and matrix rows/columns across the tabs.
/// </summary>
[ApiController]
[Route("api/reference")]
[Authorize]
public class ReferenceController : ControllerBase
{
    private readonly IReferenceService _referenceService;

    public ReferenceController(IReferenceService referenceService)
    {
        _referenceService = referenceService;
    }

    /// <summary>Returns all reference items, optionally including deactivated ones.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReferenceItemDto>>> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var items = await _referenceService.GetAllAsync(includeInactive, cancellationToken);
        return Ok(items);
    }

    /// <summary>Returns the items of a single reference list.</summary>
    [HttpGet("{type}")]
    public async Task<ActionResult<IReadOnlyList<ReferenceItemDto>>> GetByType(
        ReferenceType type,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var items = await _referenceService.GetByTypeAsync(type, includeInactive, cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ReferenceItemDto>> Create(
        [FromBody] CreateReferenceItemRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _referenceService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByType), new { type = created.Type }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateReferenceItemRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _referenceService.UpdateAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _referenceService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
