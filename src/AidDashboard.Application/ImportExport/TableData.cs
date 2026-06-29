namespace AidDashboard.Application.ImportExport;

/// <summary>
/// A simple tabular representation of a tab's data: a header row plus data rows of string cells.
/// This is the common shape used for export (to XLSX/CSV/PDF) and import (from XLSX/CSV),
/// keeping the file-format concerns separate from the per-tab mapping concerns.
/// </summary>
public class TableData
{
    public IReadOnlyList<string> Headers { get; }
    public IReadOnlyList<IReadOnlyList<string?>> Rows { get; }

    public TableData(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string?>> rows)
    {
        Headers = headers;
        Rows = rows;
    }

    /// <summary>An empty table (header row only), used for templates.</summary>
    public static TableData TemplateOf(IReadOnlyList<string> headers) =>
        new(headers, Array.Empty<IReadOnlyList<string?>>());
}

/// <summary>Supported export formats.</summary>
public enum ExportFormat
{
    Xlsx,
    Csv,
    Pdf
}
