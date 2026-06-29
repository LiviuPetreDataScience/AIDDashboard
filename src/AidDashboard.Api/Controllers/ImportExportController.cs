using AidDashboard.Application.ImportExport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>
/// Export, template-download and import endpoints for the table tabs. Exports and templates
/// are available to any authenticated user; imports require an administrator and always target
/// the account in the route.
/// </summary>
[ApiController]
[Route("api/accounts/{accountId:int}/tabs/{tabKey}")]
[Authorize]
public class ImportExportController : ControllerBase
{
    private readonly IReadOnlyDictionary<string, ITabTableProvider> _providersByKey;
    private readonly ITableSerializer _tableSerializer;

    public ImportExportController(IEnumerable<ITabTableProvider> providers, ITableSerializer tableSerializer)
    {
        _providersByKey = providers.ToDictionary(provider => provider.TabKey, StringComparer.OrdinalIgnoreCase);
        _tableSerializer = tableSerializer;
    }

    /// <summary>Exports the tab's data as an XLSX, CSV or PDF download.</summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        int accountId,
        string tabKey,
        [FromQuery] string format,
        CancellationToken cancellationToken)
    {
        if (!_providersByKey.TryGetValue(tabKey, out var provider))
        {
            return NotFound(new { message = $"Unknown tab '{tabKey}'." });
        }

        if (!TryParseFormat(format, out var exportFormat))
        {
            return BadRequest(new { message = "Format must be xlsx, csv or pdf." });
        }

        var table = await provider.ExportAsync(accountId, cancellationToken);
        var bytes = _tableSerializer.Serialize(table, exportFormat, provider.DisplayTitle);
        return File(bytes, ContentTypeFor(exportFormat), $"{FileName(provider.DisplayTitle)}.{Extension(exportFormat)}");
    }

    /// <summary>Downloads an empty import template (XLSX or CSV) with the correct headers.</summary>
    [HttpGet("template")]
    public async Task<IActionResult> Template(
        int accountId,
        string tabKey,
        [FromQuery] string format,
        CancellationToken cancellationToken)
    {
        if (!_providersByKey.TryGetValue(tabKey, out var provider))
        {
            return NotFound(new { message = $"Unknown tab '{tabKey}'." });
        }

        // Templates only make sense as editable spreadsheets, not PDF.
        if (!TryParseFormat(format, out var exportFormat) || exportFormat == ExportFormat.Pdf)
        {
            return BadRequest(new { message = "Template format must be xlsx or csv." });
        }

        var table = await provider.TemplateAsync(cancellationToken);
        var bytes = _tableSerializer.Serialize(table, exportFormat, provider.DisplayTitle);
        return File(bytes, ContentTypeFor(exportFormat), $"{FileName(provider.DisplayTitle)}-template.{Extension(exportFormat)}");
    }

    /// <summary>Imports an uploaded XLSX or CSV file into the tab for this account (administrators only).</summary>
    [HttpPost("import")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Import(
        int accountId,
        string tabKey,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (!_providersByKey.TryGetValue(tabKey, out var provider))
        {
            return NotFound(new { message = $"Unknown tab '{tabKey}'." });
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file was uploaded." });
        }

        await using var stream = file.OpenReadStream();
        var table = _tableSerializer.Parse(stream, file.FileName);
        await provider.ImportAsync(accountId, table, cancellationToken);
        return NoContent();
    }

    private static bool TryParseFormat(string? format, out ExportFormat exportFormat)
    {
        switch (format?.Trim().ToLowerInvariant())
        {
            case "xlsx":
                exportFormat = ExportFormat.Xlsx;
                return true;
            case "csv":
                exportFormat = ExportFormat.Csv;
                return true;
            case "pdf":
                exportFormat = ExportFormat.Pdf;
                return true;
            default:
                exportFormat = ExportFormat.Xlsx;
                return false;
        }
    }

    private static string ContentTypeFor(ExportFormat format) => format switch
    {
        ExportFormat.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ExportFormat.Csv => "text/csv",
        ExportFormat.Pdf => "application/pdf",
        _ => "application/octet-stream",
    };

    private static string Extension(ExportFormat format) => format switch
    {
        ExportFormat.Xlsx => "xlsx",
        ExportFormat.Csv => "csv",
        ExportFormat.Pdf => "pdf",
        _ => "bin",
    };

    private static string FileName(string title) =>
        new string(title.Select(character => char.IsLetterOrDigit(character) ? character : '-').ToArray());
}
