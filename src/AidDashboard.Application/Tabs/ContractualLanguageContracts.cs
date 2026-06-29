namespace AidDashboard.Application.Tabs;

/// <summary>One numeric cell of the Contractual Languages matrix: a value for a Location/Language pair.</summary>
public record ContractualLanguageCellDto(int LocationRefId, int LanguageRefId, double Value);

/// <summary>Reads and saves the Contractual Languages matrix for an account.</summary>
public interface IContractualLanguageService
{
    Task<IReadOnlyList<ContractualLanguageCellDto>> GetAsync(int accountId, CancellationToken cancellationToken = default);

    Task<bool> SaveAsync(int accountId, IReadOnlyList<ContractualLanguageCellDto> cells, CancellationToken cancellationToken = default);
}
