using AidDashboard.Domain.Accounts;

namespace AidDashboard.Application.Tabs;

/// <summary>One language row on the Support Hours tab.</summary>
public record SupportHourRowDto(
    int LanguageRefId,
    string? FromMondayFriday,
    string? ToMondayFriday,
    SupportCoverage Coverage,
    bool OnCallInterpretDhs,
    bool Sophie);

/// <summary>Reads and saves the Support Hours tab for an account.</summary>
public interface ISupportHourService
{
    Task<IReadOnlyList<SupportHourRowDto>> GetAsync(int accountId, CancellationToken cancellationToken = default);

    Task<bool> SaveAsync(int accountId, IReadOnlyList<SupportHourRowDto> rows, CancellationToken cancellationToken = default);
}
