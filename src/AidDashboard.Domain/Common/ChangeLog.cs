namespace AidDashboard.Domain.Common;

/// <summary>
/// Append-only change-history record. One row per create/update/delete of a tracked entity.
/// </summary>
public class ChangeLog
{
    public int Id { get; set; }
    public string EntityType { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    /// <summary>The owning account, when the change relates to account-scoped data.</summary>
    public int? AccountId { get; set; }
    /// <summary>Created, Updated or Deleted.</summary>
    public string Action { get; set; } = default!;
    /// <summary>JSON describing the changed properties (old/new values).</summary>
    public string? ChangesJson { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}
