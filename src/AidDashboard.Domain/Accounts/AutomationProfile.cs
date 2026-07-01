using AidDashboard.Domain.Common;

namespace AidDashboard.Domain.Accounts;

/// <summary>
/// Shared attributes of an automation (independent of any account): which automation reference
/// it describes plus its category, goal, affected environment and whether it uses AI. One profile
/// per automation; the Automations dashboard joins each account's rows to these attributes.
/// </summary>
public class AutomationProfile : AuditableEntity
{
    /// <summary>The Automation reference item (ReferenceType.Automation) this profile describes.</summary>
    public int AutomationRefId { get; set; }
    public int? CategoryRefId { get; set; }
    public int? GoalRefId { get; set; }
    public int? EnvironmentRefId { get; set; }
    /// <summary>Whether the automation uses AI. Null means unknown/blank.</summary>
    public bool? AiUsed { get; set; }
}
