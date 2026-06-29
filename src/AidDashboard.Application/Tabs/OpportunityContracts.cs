namespace AidDashboard.Application.Tabs;

/// <summary>A row on the Opportunities tab. Id is 0 for rows newly added on the client.</summary>
public class OpportunityDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? OpportunityTypeRefId { get; set; }
    public int? RelatedServiceRefId { get; set; }
    public int? StatusRefId { get; set; }
    public decimal? EstimatedMonthlyValue { get; set; }
    public int? EstimatedContractDurationMonths { get; set; }
    public int? DeliveredByRefId { get; set; }
    public string? Details { get; set; }
}

/// <summary>Reads and saves the Opportunities tab for an account.</summary>
public interface IOpportunityService
{
    Task<IReadOnlyList<OpportunityDto>> GetAsync(int accountId, CancellationToken cancellationToken = default);

    Task<bool> SaveAsync(int accountId, IReadOnlyList<OpportunityDto> rows, CancellationToken cancellationToken = default);
}
