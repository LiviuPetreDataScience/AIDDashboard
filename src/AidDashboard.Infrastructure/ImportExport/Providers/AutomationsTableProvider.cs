using AidDashboard.Application.ImportExport;
using AidDashboard.Application.Reference;
using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Reference;

namespace AidDashboard.Infrastructure.ImportExport.Providers;

/// <summary>Import/export mapping for the Automations tab.</summary>
public class AutomationsTableProvider : ITabTableProvider
{
    private static readonly string[] HeaderRow =
    {
        "Automation Name", "Deployment Date", "Cost of implementation (one time - $)",
        "Running cost (monthly - $)", "Efficiency impact (FTE/month)", "Delivered By", "Details",
    };

    private readonly IAutomationService _automationService;
    private readonly IReferenceService _referenceService;

    public AutomationsTableProvider(IAutomationService automationService, IReferenceService referenceService)
    {
        _automationService = automationService;
        _referenceService = referenceService;
    }

    public string TabKey => "automations";
    public string DisplayTitle => "Automations";

    public async Task<TableData> ExportAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var serviceTowers = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.ServiceTower, true, cancellationToken));
        var automations = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.Automation, true, cancellationToken));
        var rows = await _automationService.GetAsync(accountId, cancellationToken);

        var tableRows = rows
            .Select(row => (IReadOnlyList<string?>)new[]
            {
                automations.NameOf(row.AutomationRefId),
                CellParsers.FormatDate(row.DeploymentDate),
                CellParsers.FormatNumber(row.CostOfImplementationOneTime),
                CellParsers.FormatNumber(row.RunningCostMonthly),
                CellParsers.FormatNumber(row.EfficiencyImpactFtePerMonth),
                serviceTowers.NameOf(row.DeliveredByRefId),
                row.Details,
            })
            .ToList();

        return new TableData(HeaderRow, tableRows);
    }

    public Task<TableData> TemplateAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(TableData.TemplateOf(HeaderRow));

    public async Task ImportAsync(int accountId, TableData data, CancellationToken cancellationToken = default)
    {
        var serviceTowers = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.ServiceTower, true, cancellationToken));
        var automations = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.Automation, true, cancellationToken));

        var rows = data.Rows
            .Where(row => !string.IsNullOrWhiteSpace(row.At(0)))
            .Select(row => new AutomationDto
            {
                Id = 0,
                AutomationRefId = automations.IdOf(row.At(0)),
                DeploymentDate = CellParsers.Date(row.At(1)),
                CostOfImplementationOneTime = CellParsers.Decimal(row.At(2)),
                RunningCostMonthly = CellParsers.Decimal(row.At(3)),
                EfficiencyImpactFtePerMonth = CellParsers.Double(row.At(4)),
                DeliveredByRefId = serviceTowers.IdOf(row.At(5)),
                Details = CellParsers.Text(row.At(6)),
            })
            .ToList();

        await _automationService.SaveAsync(accountId, rows, cancellationToken);
    }
}
