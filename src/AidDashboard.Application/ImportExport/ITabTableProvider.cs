namespace AidDashboard.Application.ImportExport;

/// <summary>
/// Converts a single tab's data to and from <see cref="TableData"/> so it can be exported,
/// imported, and have a template generated. One implementation per importable/exportable table.
/// </summary>
public interface ITabTableProvider
{
    /// <summary>Stable key identifying the tab, used in the import/export route (e.g. "automations").</summary>
    string TabKey { get; }

    /// <summary>Human-friendly title used for file names and the PDF heading.</summary>
    string DisplayTitle { get; }

    /// <summary>Builds the export table for an account.</summary>
    Task<TableData> ExportAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>Builds an empty template (headers only) for download.</summary>
    Task<TableData> TemplateAsync(CancellationToken cancellationToken = default);

    /// <summary>Imports rows into the account, replacing the tab's current data.</summary>
    Task ImportAsync(int accountId, TableData data, CancellationToken cancellationToken = default);
}

/// <summary>Renders a <see cref="TableData"/> to a downloadable byte payload, and parses uploads back.</summary>
public interface ITableSerializer
{
    byte[] Serialize(TableData table, ExportFormat format, string title);

    /// <summary>Parses an uploaded CSV or XLSX file into a table (first row treated as headers).</summary>
    TableData Parse(Stream fileStream, string fileName);
}
