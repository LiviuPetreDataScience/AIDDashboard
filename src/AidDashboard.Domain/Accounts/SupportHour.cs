using AidDashboard.Domain.Common;

namespace AidDashboard.Domain.Accounts;

/// <summary>
/// A row on the Support Hours tab. One row per support-hours language.
/// Coverage is one of three mutually-exclusive options; the two flags are independent.
/// </summary>
public class SupportHour : AuditableEntity
{
    public int AccountId { get; set; }
    public int LanguageRefId { get; set; }       // Support Hours Language
    public string? FromMondayFriday { get; set; } // e.g. "8:00"
    public string? ToMondayFriday { get; set; }   // e.g. "18:00"
    public SupportCoverage Coverage { get; set; } = SupportCoverage.None;
    public bool OnCallInterpretDhs { get; set; }
    public bool Sophie { get; set; }
}
