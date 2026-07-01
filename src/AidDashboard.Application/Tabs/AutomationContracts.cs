namespace AidDashboard.Application.Tabs;

/// <summary>A row on the Automations tab. Id is 0 for rows newly added on the client.</summary>
public class AutomationDto
{
    public int Id { get; set; }
    public int? AutomationRefId { get; set; }
    public DateOnly? DeploymentDate { get; set; }
    public decimal? CostOfImplementationOneTime { get; set; }
    public decimal? RunningCostMonthly { get; set; }
    public double? EfficiencyImpactFtePerMonth { get; set; }
    public int? DeliveredByRefId { get; set; }
    public string? Details { get; set; }
}

/// <summary>Reads and saves the Automations tab for an account.</summary>
public interface IAutomationService
{
    Task<IReadOnlyList<AutomationDto>> GetAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>Persists the supplied rows: existing rows are updated, new rows inserted, missing rows deleted.</summary>
    Task<bool> SaveAsync(int accountId, IReadOnlyList<AutomationDto> rows, CancellationToken cancellationToken = default);
}
