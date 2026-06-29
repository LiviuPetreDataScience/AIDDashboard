using AidDashboard.Domain.Reference;

namespace AidDashboard.Application.Reference;

/// <summary>A reference list entry returned to clients.</summary>
public record ReferenceItemDto(int Id, ReferenceType Type, string Name, int SortOrder, bool IsActive);

/// <summary>Payload to create a new reference list entry (admin only).</summary>
public record CreateReferenceItemRequest(ReferenceType Type, string Name, int? SortOrder);

/// <summary>Payload to update an existing reference list entry (admin only).</summary>
public record UpdateReferenceItemRequest(string Name, int SortOrder, bool IsActive);
