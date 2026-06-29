using AidDashboard.Domain.Common;

namespace AidDashboard.Domain.Accounts;

/// <summary>A row on the SLAs &amp; KPIs tab.</summary>
public class SlaKpi : AuditableEntity
{
    public int AccountId { get; set; }
    public string? Name { get; set; }
    public int? TypeRefId { get; set; }            // SLA / KPI / HCM
    public string? Description { get; set; }
    public string? Formula { get; set; }
    public string? TargetPercent { get; set; }     // free text, e.g. "Y1: 5% Y2"
    public int? MeasurementTypeRefId { get; set; } // High / Low
    public bool CanBeReported { get; set; }
    public bool FinancialPenalties { get; set; }
    public bool Bonus { get; set; }
}
