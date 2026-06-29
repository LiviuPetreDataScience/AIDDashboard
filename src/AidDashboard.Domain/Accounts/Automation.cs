using AidDashboard.Domain.Common;

namespace AidDashboard.Domain.Accounts;

/// <summary>A row on the Automations tab.</summary>
public class Automation : AuditableEntity
{
    public int AccountId { get; set; }
    public string? Name { get; set; }
    public DateOnly? DeploymentDate { get; set; }
    public decimal? CostOfImplementationOneTime { get; set; }
    public decimal? RunningCostMonthly { get; set; }
    public double? EfficiencyImpactFtePerMonth { get; set; }
    public int? DeliveredByRefId { get; set; }   // Service Tower
    public string? Details { get; set; }
}
