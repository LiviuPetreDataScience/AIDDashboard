using AidDashboard.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>
/// The hierarchical Services catalog. Reads are open to any authenticated user (the Services tab
/// needs the leaf list); editing the hierarchy is admin-only (used by the admin drill-down editor).
/// </summary>
[ApiController]
[Route("api/service-catalog")]
[Authorize]
public class ServiceCatalogController : ControllerBase
{
    private readonly IServiceCatalogService _serviceCatalog;

    public ServiceCatalogController(IServiceCatalogService serviceCatalog)
    {
        _serviceCatalog = serviceCatalog;
    }

    /// <summary>Items directly under a parent (omit parentId for the top-level categories).</summary>
    [HttpGet("children")]
    public async Task<ActionResult<IReadOnlyList<ServiceCatalogItemDto>>> GetChildren(
        [FromQuery] int? parentId,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var items = await _serviceCatalog.GetChildrenAsync(parentId, includeInactive, cancellationToken);
        return Ok(items);
    }

    /// <summary>The breadcrumb (ancestor names) for an item, for the admin editor.</summary>
    [HttpGet("{id:int}/breadcrumb")]
    public async Task<ActionResult<IReadOnlyList<BreadcrumbItemDto>>> GetBreadcrumb(int id, CancellationToken cancellationToken)
    {
        var breadcrumb = await _serviceCatalog.GetBreadcrumbAsync(id, cancellationToken);
        return Ok(breadcrumb);
    }

    /// <summary>All leaf services with their bundled path (used to build the Services tab rows).</summary>
    [HttpGet("leaves")]
    public async Task<ActionResult<IReadOnlyList<ServiceLeafDto>>> GetLeaves(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var leaves = await _serviceCatalog.GetLeafServicesAsync(includeInactive, cancellationToken);
        return Ok(leaves);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ServiceCatalogItemDto>> Create([FromBody] CreateServiceItemRequest request, CancellationToken cancellationToken)
    {
        var created = await _serviceCatalog.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetChildren), new { parentId = created.ParentId }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceItemRequest request, CancellationToken cancellationToken)
    {
        var updated = await _serviceCatalog.UpdateAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _serviceCatalog.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
