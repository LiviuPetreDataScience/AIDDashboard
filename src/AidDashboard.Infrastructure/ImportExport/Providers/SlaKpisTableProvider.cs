using AidDashboard.Application.ImportExport;
using AidDashboard.Application.Reference;
using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Reference;

namespace AidDashboard.Infrastructure.ImportExport.Providers;

/// <summary>Import/export mapping for the SLAs &amp; KPIs tab.</summary>
public class SlaKpisTableProvider : ITabTableProvider
{
    private static readonly string[] HeaderRow =
    {
        "Name", "Type", "Description", "Formula", "Target %", "Measurement type",
        "Can it be reported?", "Financial penalties", "Bonus",
    };

    private readonly ISlaKpiService _slaKpiService;
    private readonly IReferenceService _referenceService;

    public SlaKpisTableProvider(ISlaKpiService slaKpiService, IReferenceService referenceService)
    {
        _slaKpiService = slaKpiService;
        _referenceService = referenceService;
    }

    public string TabKey => "sla-kpis";
    public string DisplayTitle => "SLAs and KPIs";

    public async Task<TableData> ExportAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var types = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.SlaType, true, cancellationToken));
        var measurements = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.MeasurementType, true, cancellationToken));

        var rows = await _slaKpiService.GetAsync(accountId, cancellationToken);

        var tableRows = rows
            .Select(row => (IReadOnlyList<string?>)new[]
            {
                row.Name,
                types.NameOf(row.TypeRefId),
                row.Description,
                row.Formula,
                row.TargetPercent,
                measurements.NameOf(row.MeasurementTypeRefId),
                CellParsers.FormatYesNo(row.CanBeReported),
                CellParsers.FormatYesNo(row.FinancialPenalties),
                CellParsers.FormatYesNo(row.Bonus),
            })
            .ToList();

        return new TableData(HeaderRow, tableRows);
    }

    public Task<TableData> TemplateAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(TableData.TemplateOf(HeaderRow));

    public async Task ImportAsync(int accountId, TableData data, CancellationToken cancellationToken = default)
    {
        var types = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.SlaType, true, cancellationToken));
        var measurements = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.MeasurementType, true, cancellationToken));

        var rows = data.Rows
            .Where(row => !string.IsNullOrWhiteSpace(row.At(0)))
            .Select(row => new SlaKpiDto
            {
                Id = 0,
                Name = CellParsers.Text(row.At(0)),
                TypeRefId = types.IdOf(row.At(1)),
                Description = CellParsers.Text(row.At(2)),
                Formula = CellParsers.Text(row.At(3)),
                TargetPercent = CellParsers.Text(row.At(4)),
                MeasurementTypeRefId = measurements.IdOf(row.At(5)),
                CanBeReported = CellParsers.Bool(row.At(6)),
                FinancialPenalties = CellParsers.Bool(row.At(7)),
                Bonus = CellParsers.Bool(row.At(8)),
            })
            .ToList();

        await _slaKpiService.SaveAsync(accountId, rows, cancellationToken);
    }
}
