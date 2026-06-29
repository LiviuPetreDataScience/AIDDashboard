using AidDashboard.Domain.Reference;

namespace AidDashboard.Application.Reference;

/// <summary>Reads and (for admins) maintains the reference lists used across the tabs.</summary>
public interface IReferenceService
{
    /// <summary>Returns every reference item, optionally including deactivated ones.</summary>
    Task<IReadOnlyList<ReferenceItemDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>Returns the items of a single reference list.</summary>
    Task<IReadOnlyList<ReferenceItemDto>> GetByTypeAsync(ReferenceType type, bool includeInactive = false, CancellationToken cancellationToken = default);

    Task<ReferenceItemDto> CreateAsync(CreateReferenceItemRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(int id, UpdateReferenceItemRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
