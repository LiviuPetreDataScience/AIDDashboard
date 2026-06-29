using AidDashboard.Application.ImportExport;
using AidDashboard.Application.Reference;
using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Accounts;
using AidDashboard.Domain.Reference;

namespace AidDashboard.Infrastructure.ImportExport.Providers;

/// <summary>Import/export mapping for the Support Hours tab.</summary>
public class SupportHoursTableProvider : ITabTableProvider
{
    private static readonly string[] HeaderRow =
    {
        "Language", "From (M-F)", "To (M-F)", "Coverage (8x5 / 24x5 / 24x7)",
        "On call / Interpret (DHS)", "Sophie",
    };

    private static readonly Dictionary<SupportCoverage, string> CoverageToText = new()
    {
        [SupportCoverage.None] = "",
        [SupportCoverage.EightByFive] = "8x5",
        [SupportCoverage.TwentyFourFive] = "24/5",
        [SupportCoverage.TwentyFourSeven] = "24/7",
    };

    private readonly ISupportHourService _supportHourService;
    private readonly IReferenceService _referenceService;

    public SupportHoursTableProvider(ISupportHourService supportHourService, IReferenceService referenceService)
    {
        _supportHourService = supportHourService;
        _referenceService = referenceService;
    }

    public string TabKey => "support-hours";
    public string DisplayTitle => "Support Hours";

    public async Task<TableData> ExportAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var languages = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.SupportHoursLanguage, true, cancellationToken));
        var rows = await _supportHourService.GetAsync(accountId, cancellationToken);

        var tableRows = rows
            .Select(row => (IReadOnlyList<string?>)new[]
            {
                languages.NameOf(row.LanguageRefId),
                row.FromMondayFriday,
                row.ToMondayFriday,
                CoverageToText[row.Coverage],
                CellParsers.FormatYesNo(row.OnCallInterpretDhs),
                CellParsers.FormatYesNo(row.Sophie),
            })
            .ToList();

        return new TableData(HeaderRow, tableRows);
    }

    public Task<TableData> TemplateAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(TableData.TemplateOf(HeaderRow));

    public async Task ImportAsync(int accountId, TableData data, CancellationToken cancellationToken = default)
    {
        var languages = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.SupportHoursLanguage, true, cancellationToken));

        var rows = new List<SupportHourRowDto>();
        foreach (var row in data.Rows)
        {
            var languageId = languages.IdOf(row.At(0));
            if (languageId is null)
            {
                continue;
            }

            rows.Add(new SupportHourRowDto(
                languageId.Value,
                CellParsers.Text(row.At(1)),
                CellParsers.Text(row.At(2)),
                ParseCoverage(row.At(3)),
                CellParsers.Bool(row.At(4)),
                CellParsers.Bool(row.At(5))));
        }

        await _supportHourService.SaveAsync(accountId, rows, cancellationToken);
    }

    private static SupportCoverage ParseCoverage(string? text)
    {
        var value = text?.Trim();
        return value switch
        {
            "8x5" => SupportCoverage.EightByFive,
            "24/5" => SupportCoverage.TwentyFourFive,
            "24/7" => SupportCoverage.TwentyFourSeven,
            _ => SupportCoverage.None,
        };
    }
}
