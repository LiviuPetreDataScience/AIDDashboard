using AidDashboard.Domain.Accounts;

namespace AidDashboard.Application.Tabs;

/// <summary>One numeric cell of a staffing model: a value for a Location/Role pair.</summary>
public record StaffingCellDto(int LocationRefId, int RoleRefId, double Value);

/// <summary>Reads and saves the Initial and Latest Approved staffing models for an account.</summary>
public interface IStaffingService
{
    Task<IReadOnlyList<StaffingCellDto>> GetAsync(int accountId, StaffingModelType modelType, CancellationToken cancellationToken = default);

    /// <summary>Replaces the model's cells with the supplied set (upsert by Location/Role, removing the rest).</summary>
    Task<bool> SaveAsync(int accountId, StaffingModelType modelType, IReadOnlyList<StaffingCellDto> cells, CancellationToken cancellationToken = default);
}
