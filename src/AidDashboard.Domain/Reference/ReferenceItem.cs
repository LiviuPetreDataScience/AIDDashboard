using AidDashboard.Domain.Common;

namespace AidDashboard.Domain.Reference;

/// <summary>
/// A single entry in a reference list (e.g. a Location, a Device, an Opportunity Status).
/// All reference lists live in one table, partitioned by <see cref="Type"/>.
/// </summary>
public class ReferenceItem : AuditableEntity
{
    public ReferenceType Type { get; set; }
    public string Name { get; set; } = default!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
