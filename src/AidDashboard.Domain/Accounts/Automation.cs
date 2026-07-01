using AidDashboard.Domain.Common;

namespace AidDashboard.Domain.Accounts;

/// <summary>A row on the Automations tab.</summary>
public class Automation : AuditableEntity
{
    public int AccountId { get; set; }
    /// <summary>The automation, chosen from the Automation reference list (enables grouping across accounts).</summary>
    public int? AutomationRefId { get; set; }
    public DateOnly? DeploymentDate { get; set; }
    public decimal? CostOfImplementationOneTime { get; set; }
    public decimal? RunningCostMonthly { get; set; }
    public double? EfficiencyImpactFtePerMonth { get; set; }
    public int? DeliveredByRefId { get; set; }   // Service Tower
    public string? Details { get; set; }
}
