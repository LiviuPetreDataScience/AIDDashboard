namespace AidDashboard.Application.Accounts;

/// <summary>Manages accounts and their "Overall" tab data.</summary>
public interface IAccountService
{
    Task<IReadOnlyList<AccountSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AccountSummaryDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);

    Task<AccountOverallDto?> GetOverallAsync(int accountId, CancellationToken cancellationToken = default);

    Task<bool> UpdateOverallAsync(int accountId, AccountOverallDto overall, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int accountId, CancellationToken cancellationToken = default);
}
