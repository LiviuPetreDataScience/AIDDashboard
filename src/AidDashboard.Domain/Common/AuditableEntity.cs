namespace AidDashboard.Domain.Common;

/// <summary>
/// Base type for entities that carry creation/modification audit metadata.
/// Populated automatically by the persistence layer on SaveChanges.
/// </summary>
public abstract class AuditableEntity
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}
