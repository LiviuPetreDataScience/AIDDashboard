using AidDashboard.Domain.Common;

namespace AidDashboard.Domain.Accounts;

/// <summary>A row on the Opportunities tab.</summary>
public class Opportunity : AuditableEntity
{
    public int AccountId { get; set; }
    public string? Name { get; set; }
    public int? OpportunityTypeRefId { get; set; }
    public int? RelatedServiceRefId { get; set; }
    public int? StatusRefId { get; set; }
    public decimal? EstimatedMonthlyValue { get; set; }
    public int? EstimatedContractDurationMonths { get; set; }
    public int? DeliveredByRefId { get; set; }   // Service Tower
    public string? Details { get; set; }
}
