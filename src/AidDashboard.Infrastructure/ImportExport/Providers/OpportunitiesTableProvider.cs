using AidDashboard.Application.ImportExport;
using AidDashboard.Application.Reference;
using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Reference;

namespace AidDashboard.Infrastructure.ImportExport.Providers;

/// <summary>Import/export mapping for the Opportunities tab.</summary>
public class OpportunitiesTableProvider : ITabTableProvider
{
    private static readonly string[] HeaderRow =
    {
        "Opportunity Name", "Opportunity Type", "Related service", "Status",
        "Estimated monthly value ($)", "Estimated contract duration (Months)", "Delivered By", "Details",
    };

    private readonly IOpportunityService _opportunityService;
    private readonly IReferenceService _referenceService;

    public OpportunitiesTableProvider(IOpportunityService opportunityService, IReferenceService referenceService)
    {
        _opportunityService = opportunityService;
        _referenceService = referenceService;
    }

    public string TabKey => "opportunities";
    public string DisplayTitle => "Opportunities";

    public async Task<TableData> ExportAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var types = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.OpportunityType, true, cancellationToken));
        var services = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.RelatedService, true, cancellationToken));
        var statuses = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.OpportunityStatus, true, cancellationToken));
        var towers = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.ServiceTower, true, cancellationToken));

        var rows = await _opportunityService.GetAsync(accountId, cancellationToken);

        var tableRows = rows
            .Select(row => (IReadOnlyList<string?>)new[]
            {
                row.Name,
                types.NameOf(row.OpportunityTypeRefId),
                services.NameOf(row.RelatedServiceRefId),
                statuses.NameOf(row.StatusRefId),
                CellParsers.FormatNumber(row.EstimatedMonthlyValue),
                CellParsers.FormatNumber(row.EstimatedContractDurationMonths),
                towers.NameOf(row.DeliveredByRefId),
                row.Details,
            })
            .ToList();

        return new TableData(HeaderRow, tableRows);
    }

    public Task<TableData> TemplateAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(TableData.TemplateOf(HeaderRow));

    public async Task ImportAsync(int accountId, TableData data, CancellationToken cancellationToken = default)
    {
        var types = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.OpportunityType, true, cancellationToken));
        var services = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.RelatedService, true, cancellationToken));
        var statuses = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.OpportunityStatus, true, cancellationToken));
        var towers = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.ServiceTower, true, cancellationToken));

        var rows = data.Rows
            .Where(row => !string.IsNullOrWhiteSpace(row.At(0)))
            .Select(row => new OpportunityDto
            {
                Id = 0,
                Name = CellParsers.Text(row.At(0)),
                OpportunityTypeRefId = types.IdOf(row.At(1)),
                RelatedServiceRefId = services.IdOf(row.At(2)),
                StatusRefId = statuses.IdOf(row.At(3)),
                EstimatedMonthlyValue = CellParsers.Decimal(row.At(4)),
                EstimatedContractDurationMonths = CellParsers.Int(row.At(5)),
                DeliveredByRefId = towers.IdOf(row.At(6)),
                Details = CellParsers.Text(row.At(7)),
            })
            .ToList();

        await _opportunityService.SaveAsync(accountId, rows, cancellationToken);
    }
}
