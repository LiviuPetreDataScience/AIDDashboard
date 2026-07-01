using AidDashboard.Domain.Common;

namespace AidDashboard.Domain.Services;

/// <summary>Whether a catalog node is a (sub)category or a leaf service.</summary>
public enum ServiceNodeKind
{
    Category = 0,
    Service = 1
}

/// <summary>
/// A node in the Services hierarchy. Top-level nodes (ParentId null) are categories; nodes can
/// nest to any depth; leaf nodes of kind <see cref="ServiceNodeKind.Service"/> are the actual
/// services shown on the Services tab. A category may contain both sub-categories and services.
/// </summary>
public class ServiceCatalogItem : AuditableEntity
{
    public int? ParentId { get; set; }
    public string Name { get; set; } = default!;
    public ServiceNodeKind Kind { get; set; } = ServiceNodeKind.Category;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public List<ServiceCatalogItem> Children { get; set; } = new();
}
