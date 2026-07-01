using AidDashboard.Application.ImportExport;
using AidDashboard.Application.Reference;
using AidDashboard.Application.Services;
using AidDashboard.Domain.Reference;

namespace AidDashboard.Infrastructure.ImportExport.Providers;

/// <summary>Import/export mapping for the Services tab.</summary>
public class ServicesTableProvider : ITabTableProvider
{
    private static readonly string[] HeaderRow =
    {
        "Path", "Service", "Contract end date (SOW)", "Does this service exist in the company ?",
        "Is it provided by Stefanini Group ?",
        "If not provided by SG then by who (by client or by a competitor provider) ?",
        "If service is not existent, is there an opportunity to provide it ?", "Details",
    };

    private readonly IAccountServicesService _accountServices;
    private readonly IServiceCatalogService _catalog;
    private readonly IReferenceService _referenceService;

    public ServicesTableProvider(
        IAccountServicesService accountServices,
        IServiceCatalogService catalog,
        IReferenceService referenceService)
    {
        _accountServices = accountServices;
        _catalog = catalog;
        _referenceService = referenceService;
    }

    public string TabKey => "services";
    public string DisplayTitle => "Services";

    public async Task<TableData> ExportAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var competitors = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.Competitor, true, cancellationToken));
        var rows = await _accountServices.GetAsync(accountId, cancellationToken);

        var tableRows = rows
            .Select(row => (IReadOnlyList<string?>)new[]
            {
                row.Path,
                row.ServiceName,
                CellParsers.FormatDate(row.ContractEndDate),
                CellParsers.FormatYesNo(row.ExistsInCompany),
                CellParsers.FormatYesNo(row.ProvidedByStefaniniGroup),
                competitors.NameOf(row.ProvidedByRefId),
                CellParsers.FormatYesNo(row.OpportunityToProvide),
                row.Details,
            })
            .ToList();

        return new TableData(HeaderRow, tableRows);
    }

    public Task<TableData> TemplateAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(TableData.TemplateOf(HeaderRow));

    public async Task ImportAsync(int accountId, TableData data, CancellationToken cancellationToken = default)
    {
        var competitors = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.Competitor, true, cancellationToken));
        var leaves = await _catalog.GetLeafServicesAsync(true, cancellationToken);

        // Match an imported row to a service by its (path, service name) pair.
        var serviceIdByPathAndName = new Dictionary<(string Path, string Name), int>();
        foreach (var leaf in leaves)
        {
            serviceIdByPathAndName[(leaf.Path.Trim(), leaf.ServiceName.Trim())] = leaf.Id;
        }

        var rows = new List<AccountServiceRowDto>();
        foreach (var row in data.Rows)
        {
            var path = (row.At(0) ?? string.Empty).Trim();
            var name = (row.At(1) ?? string.Empty).Trim();
            if (!serviceIdByPathAndName.TryGetValue((path, name), out var serviceItemId))
            {
                continue; // unknown service — skip
            }

            rows.Add(new AccountServiceRowDto
            {
                ServiceItemId = serviceItemId,
                ContractEndDate = CellParsers.Date(row.At(2)),
                ExistsInCompany = CellParsers.YesNo(row.At(3)),
                ProvidedByStefaniniGroup = CellParsers.YesNo(row.At(4)),
                ProvidedByRefId = competitors.IdOf(row.At(5)),
                OpportunityToProvide = CellParsers.YesNo(row.At(6)),
                Details = CellParsers.Text(row.At(7)),
            });
        }

        await _accountServices.SaveAsync(accountId, rows, cancellationToken);
    }
}
