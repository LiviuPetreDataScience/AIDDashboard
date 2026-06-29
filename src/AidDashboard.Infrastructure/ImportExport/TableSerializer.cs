using System.Globalization;
using System.Text;
using AidDashboard.Application.ImportExport;
using ClosedXML.Excel;
using CsvHelper;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace AidDashboard.Infrastructure.ImportExport;

/// <inheritdoc cref="ITableSerializer"/>
public class TableSerializer : ITableSerializer
{
    static TableSerializer()
    {
        // Register the font resolver once so PDF export can render text on any platform.
        GlobalFontSettings.FontResolver ??= new DefaultFontResolver();
    }

    public byte[] Serialize(TableData table, ExportFormat format, string title) => format switch
    {
        ExportFormat.Xlsx => SerializeXlsx(table, title),
        ExportFormat.Csv => SerializeCsv(table),
        ExportFormat.Pdf => SerializePdf(table, title),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format."),
    };

    public TableData Parse(Stream fileStream, string fileName)
    {
        var isExcel = fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
        return isExcel ? ParseXlsx(fileStream) : ParseCsv(fileStream);
    }

    // ---- Excel ----

    private static byte[] SerializeXlsx(TableData table, string title)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(SafeSheetName(title));

        for (var column = 0; column < table.Headers.Count; column++)
        {
            var headerCell = worksheet.Cell(1, column + 1);
            headerCell.Value = table.Headers[column];
            headerCell.Style.Font.Bold = true;
        }

        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var row = table.Rows[rowIndex];
            for (var column = 0; column < row.Count; column++)
            {
                worksheet.Cell(rowIndex + 2, column + 1).Value = row[column] ?? string.Empty;
            }
        }

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    private static TableData ParseXlsx(Stream fileStream)
    {
        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheet(1);
        var usedRange = worksheet.RangeUsed();
        if (usedRange is null)
        {
            return new TableData(Array.Empty<string>(), Array.Empty<IReadOnlyList<string?>>());
        }

        var rows = usedRange.RowsUsed().ToList();
        var headers = rows[0].Cells().Select(cell => cell.GetString()).ToList();

        var dataRows = new List<IReadOnlyList<string?>>();
        foreach (var row in rows.Skip(1))
        {
            var cells = new List<string?>();
            for (var column = 1; column <= headers.Count; column++)
            {
                cells.Add(row.Cell(column).GetString());
            }
            dataRows.Add(cells);
        }

        return new TableData(headers, dataRows);
    }

    // ---- CSV ----

    private static byte[] SerializeCsv(TableData table)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(true)))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            foreach (var header in table.Headers)
            {
                csv.WriteField(header);
            }
            csv.NextRecord();

            foreach (var row in table.Rows)
            {
                foreach (var cell in row)
                {
                    csv.WriteField(cell ?? string.Empty);
                }
                csv.NextRecord();
            }
        }
        return memoryStream.ToArray();
    }

    private static TableData ParseCsv(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        using var parser = new CsvParser(reader, CultureInfo.InvariantCulture);

        var rows = new List<string[]>();
        while (parser.Read())
        {
            rows.Add(parser.Record ?? Array.Empty<string>());
        }

        if (rows.Count == 0)
        {
            return new TableData(Array.Empty<string>(), Array.Empty<IReadOnlyList<string?>>());
        }

        var headers = rows[0].ToList();
        var dataRows = rows.Skip(1).Select(row => (IReadOnlyList<string?>)row.Cast<string?>().ToList()).ToList();
        return new TableData(headers, dataRows);
    }

    // ---- PDF (best-effort tabular layout) ----

    private static byte[] SerializePdf(TableData table, string title)
    {
        if (!DefaultFontResolver.IsFontAvailable)
        {
            throw new InvalidOperationException("PDF export is unavailable because no system font was found on the server.");
        }

        using var document = new PdfDocument();
        var font = new XFont("AidDefault", 8);
        var headerFont = new XFont("AidDefault", 8, XFontStyleEx.Bold);
        var titleFont = new XFont("AidDefault", 14, XFontStyleEx.Bold);

        const double margin = 24;
        const double rowHeight = 18;

        PdfPage page = NewLandscapePage(document);
        var gfx = XGraphics.FromPdfPage(page);
        var usableWidth = page.Width.Point - 2 * margin;
        var columnWidth = table.Headers.Count > 0 ? usableWidth / table.Headers.Count : usableWidth;

        gfx.DrawString(title, titleFont, XBrushes.Black, new XRect(margin, margin, usableWidth, 20), XStringFormats.TopLeft);

        double y = margin + 28;
        y = DrawRow(gfx, table.Headers, margin, y, columnWidth, rowHeight, headerFont, isHeader: true);

        foreach (var row in table.Rows)
        {
            if (y + rowHeight > page.Height.Point - margin)
            {
                gfx.Dispose();
                page = NewLandscapePage(document);
                gfx = XGraphics.FromPdfPage(page);
                y = margin;
                y = DrawRow(gfx, table.Headers, margin, y, columnWidth, rowHeight, headerFont, isHeader: true);
            }

            var cells = row.Select(cell => cell ?? string.Empty).ToList();
            y = DrawRow(gfx, cells, margin, y, columnWidth, rowHeight, font, isHeader: false);
        }

        gfx.Dispose();

        using var memoryStream = new MemoryStream();
        document.Save(memoryStream);
        return memoryStream.ToArray();
    }

    private static PdfPage NewLandscapePage(PdfDocument document)
    {
        var page = document.AddPage();
        page.Orientation = PdfSharp.PageOrientation.Landscape;
        return page;
    }

    private static double DrawRow(
        XGraphics gfx,
        IReadOnlyList<string> cells,
        double x,
        double y,
        double columnWidth,
        double rowHeight,
        XFont font,
        bool isHeader)
    {
        if (isHeader)
        {
            gfx.DrawRectangle(XBrushes.LightGray, x, y, columnWidth * cells.Count, rowHeight);
        }

        for (var column = 0; column < cells.Count; column++)
        {
            var cellRect = new XRect(x + column * columnWidth + 2, y + 2, columnWidth - 4, rowHeight - 4);
            gfx.DrawRectangle(XPens.Gainsboro, x + column * columnWidth, y, columnWidth, rowHeight);
            gfx.DrawString(Truncate(cells[column]), font, XBrushes.Black, cellRect, XStringFormats.CenterLeft);
        }

        return y + rowHeight;
    }

    /// <summary>Trims long cell text so it fits a fixed-width PDF column.</summary>
    private static string Truncate(string value) =>
        value.Length <= 28 ? value : value[..27] + "…";

    private static string SafeSheetName(string title)
    {
        var cleaned = new string(title.Where(character => !"[]:*?/\\".Contains(character)).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "Data" : cleaned[..Math.Min(31, cleaned.Length)];
    }
}
