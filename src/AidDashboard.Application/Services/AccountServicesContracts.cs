namespace AidDashboard.Application.Services;

/// <summary>A row on the Services tab: a leaf service (path + name) plus the account's data for it.</summary>
public class AccountServiceRowDto
{
    public int ServiceItemId { get; set; }
    public string Path { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateOnly? ContractEndDate { get; set; }
    public bool? ExistsInCompany { get; set; }
    public bool? ProvidedByStefaniniGroup { get; set; }
    public int? ProvidedByRefId { get; set; }
    public bool? OpportunityToProvide { get; set; }
    public string? Details { get; set; }
}

/// <summary>Reads and saves the Services tab data for an account.</summary>
public interface IAccountServicesService
{
    Task<IReadOnlyList<AccountServiceRowDto>> GetAsync(int accountId, CancellationToken cancellationToken = default);

    Task<bool> SaveAsync(int accountId, IReadOnlyList<AccountServiceRowDto> rows, CancellationToken cancellationToken = default);
}
