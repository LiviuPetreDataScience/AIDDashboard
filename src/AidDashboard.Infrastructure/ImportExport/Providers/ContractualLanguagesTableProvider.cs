using AidDashboard.Application.ImportExport;
using AidDashboard.Application.Reference;
using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Reference;

namespace AidDashboard.Infrastructure.ImportExport.Providers;

/// <summary>Import/export mapping for the Contractual Languages matrix.</summary>
public class ContractualLanguagesTableProvider : ITabTableProvider
{
    private readonly IContractualLanguageService _contractualLanguageService;
    private readonly IReferenceService _referenceService;

    public ContractualLanguagesTableProvider(
        IContractualLanguageService contractualLanguageService,
        IReferenceService referenceService)
    {
        _contractualLanguageService = contractualLanguageService;
        _referenceService = referenceService;
    }

    public string TabKey => "contractual-languages";
    public string DisplayTitle => "Contractual Languages";

    public async Task<TableData> ExportAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var locations = await _referenceService.GetByTypeAsync(ReferenceType.Location, false, cancellationToken);
        var languages = await _referenceService.GetByTypeAsync(ReferenceType.ContractualLanguage, false, cancellationToken);
        var cells = await _contractualLanguageService.GetAsync(accountId, cancellationToken);

        var matrixCells = cells
            .Select(cell => new MatrixCellValue(cell.LocationRefId, cell.LanguageRefId, cell.Value))
            .ToList();

        return MatrixTableHelper.Export("Location", locations, languages, matrixCells);
    }

    public async Task<TableData> TemplateAsync(CancellationToken cancellationToken = default)
    {
        var languages = await _referenceService.GetByTypeAsync(ReferenceType.ContractualLanguage, false, cancellationToken);
        return MatrixTableHelper.Template("Location", languages);
    }

    public async Task ImportAsync(int accountId, TableData data, CancellationToken cancellationToken = default)
    {
        var locationLookup = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.Location, true, cancellationToken));
        var languageLookup = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.ContractualLanguage, true, cancellationToken));

        var cells = MatrixTableHelper.Import(data, locationLookup, languageLookup)
            .Select(cell => new ContractualLanguageCellDto(cell.RowRefId, cell.ColRefId, cell.Value))
            .ToList();

        await _contractualLanguageService.SaveAsync(accountId, cells, cancellationToken);
    }
}
