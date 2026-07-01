using AidDashboard.Domain.Services;

namespace AidDashboard.Application.Services;

/// <summary>A node in the services hierarchy, as returned to the admin drill-down editor.</summary>
public record ServiceCatalogItemDto(int Id, int? ParentId, string Name, ServiceNodeKind Kind, int SortOrder, bool IsActive, bool HasChildren);

/// <summary>A leaf service together with its bundled category path (e.g. "Cat -> Sub -> Sub3").</summary>
public record ServiceLeafDto(int Id, string ServiceName, string Path);

/// <summary>One step of an item's path from the top, used for the admin breadcrumb.</summary>
public record BreadcrumbItemDto(int Id, string Name);

public record CreateServiceItemRequest(int? ParentId, string Name, ServiceNodeKind Kind);

public record UpdateServiceItemRequest(string Name, int SortOrder, bool IsActive);

/// <summary>Reads and (for admins) maintains the hierarchical Services catalog.</summary>
public interface IServiceCatalogService
{
    /// <summary>Items directly under a parent (null = top-level categories), for the drill-down editor.</summary>
    Task<IReadOnlyList<ServiceCatalogItemDto>> GetChildrenAsync(int? parentId, bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>The chain of ancestor names from the top down to (and including) the given item.</summary>
    Task<IReadOnlyList<BreadcrumbItemDto>> GetBreadcrumbAsync(int itemId, CancellationToken cancellationToken = default);

    /// <summary>All leaf services with their bundled path, in catalog order (used by the Services tab).</summary>
    Task<IReadOnlyList<ServiceLeafDto>> GetLeafServicesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    Task<ServiceCatalogItemDto> CreateAsync(CreateServiceItemRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(int id, UpdateServiceItemRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
