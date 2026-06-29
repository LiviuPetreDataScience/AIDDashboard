namespace AidDashboard.Application.Tabs;

/// <summary>A row on the SLAs &amp; KPIs tab. Id is 0 for rows newly added on the client.</summary>
public class SlaKpiDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? TypeRefId { get; set; }
    public string? Description { get; set; }
    public string? Formula { get; set; }
    public string? TargetPercent { get; set; }
    public int? MeasurementTypeRefId { get; set; }
    public bool CanBeReported { get; set; }
    public bool FinancialPenalties { get; set; }
    public bool Bonus { get; set; }
}

/// <summary>Reads and saves the SLAs &amp; KPIs tab for an account.</summary>
public interface ISlaKpiService
{
    Task<IReadOnlyList<SlaKpiDto>> GetAsync(int accountId, CancellationToken cancellationToken = default);

    Task<bool> SaveAsync(int accountId, IReadOnlyList<SlaKpiDto> rows, CancellationToken cancellationToken = default);
}
