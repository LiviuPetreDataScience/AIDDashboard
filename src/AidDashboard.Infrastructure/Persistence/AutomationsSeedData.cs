namespace AidDashboard.Infrastructure.Persistence;

/// <summary>Seed attributes for an automation: category, goal, environment, AI usage (any may be blank).</summary>
public sealed record AutomationAttributeSeed(string? Category, string? Goal, string? Environment, bool? AiUsed);

/// <summary>
/// Sample per-automation attributes, taken from the source screenshots. Only the automations shown
/// there are populated; everything else is left blank (as in the real data, most are blank).
/// Keyed by the automation name (must match a ReferenceType.Automation reference item).
/// </summary>
public static class AutomationsSeedData
{
    public static readonly IReadOnlyDictionary<string, AutomationAttributeSeed> Attributes =
        new Dictionary<string, AutomationAttributeSeed>
        {
            ["Sophie-ITSM tool"] = new("Sophie", null, null, true),
            ["Sophie-KB search"] = new("Sophie", "Efficiency", "Knowledge Portals, ITSM Tool", true),
            ["Sophie X- Account Management"] = new("Sophie", "Cost Saving", "Active Directory", true),
            ["EMAIL AUTOMATION"] = new("Process automation", "Cost Saving", "ITSM tool", false),
            ["Onboarding automation"] = new("Process automation", "Cost Saving", null, false),
            ["Queue Management"] = new("Process automation", "SLA Improvement", "ITSM tool", false),
            ["Automated IMS (interaction) creation"] = new("Process automation", "Cost Saving", null, false),
            ["Follow-up for user input (Servicenow)"] = new("Process automation", "Efficiency", "ITSM tool", false),
            ["SAI-Quality"] = new("Process automation", "SLA Improvement", "ITSM, Contact center tool", true),
            ["ITAM - Stockroom Auditing"] = new("Process automation", "Efficiency", "N/A", true),
            ["AMP Portal"] = new("Self service", "Cost Saving", "N/A", false),
            ["Knowledge Max"] = new("Assistants", "Efficiency", "Knowledge Portals", true),
        };
}
