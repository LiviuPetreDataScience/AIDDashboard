namespace AidDashboard.Application.Dashboard;

/// <summary>
/// One automation occurrence for an account, flattened with everything the Automations dashboard
/// needs (account context + the automation's shared attributes + the per-account figures). The
/// dashboard fetches all facts once and does its filtering/aggregation on the client.
/// </summary>
public class AutomationFactDto
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string? RegionalOwnership { get; set; }

    public int? AutomationRefId { get; set; }
    public string? AutomationName { get; set; }
    public string? Category { get; set; }
    public string? Goal { get; set; }
    public string? Environment { get; set; }
    public bool? AiUsed { get; set; }

    public string? DeliveredBy { get; set; }
    public string? DeploymentDate { get; set; }
    public decimal? CostOfImplementation { get; set; }
    public decimal? RunningCost { get; set; }
    public double? EfficiencyImpact { get; set; }
}

/// <summary>
/// The editable shared attributes of one automation (the admin "Automation Attributes" editor).
/// There is one row per Automation reference item; the attribute ids may be null (blank).
/// </summary>
public class AutomationProfileDto
{
    public int AutomationRefId { get; set; }
    public string AutomationName { get; set; } = string.Empty;
    public int? CategoryRefId { get; set; }
    public int? GoalRefId { get; set; }
    public int? EnvironmentRefId { get; set; }
    public bool? AiUsed { get; set; }
}

/// <summary>The attribute values to store for one automation (any field may be null to blank it).</summary>
public class UpdateAutomationProfileRequest
{
    public int? CategoryRefId { get; set; }
    public int? GoalRefId { get; set; }
    public int? EnvironmentRefId { get; set; }
    public bool? AiUsed { get; set; }
}

public interface IDashboardService
{
    /// <summary>Returns one fact per automation row across all accounts.</summary>
    Task<IReadOnlyList<AutomationFactDto>> GetAutomationFactsAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns one attribute profile per Automation reference item (for the admin editor).</summary>
    Task<IReadOnlyList<AutomationProfileDto>> GetAutomationProfilesAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the attribute profile for a single automation. Returns false if the automation does not exist.</summary>
    Task<bool> SaveAutomationProfileAsync(int automationRefId, UpdateAutomationProfileRequest request, CancellationToken cancellationToken = default);
}
