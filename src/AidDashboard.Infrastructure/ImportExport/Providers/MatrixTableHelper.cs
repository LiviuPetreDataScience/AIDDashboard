using AidDashboard.Application.ImportExport;
using AidDashboard.Application.Reference;

namespace AidDashboard.Infrastructure.ImportExport.Providers;

/// <summary>A normalized matrix cell used by the staffing and contractual-language providers.</summary>
public readonly record struct MatrixCellValue(int RowRefId, int ColRefId, double Value);

/// <summary>
/// Shared export/import logic for matrix tabs (row-reference x column-reference = number),
/// keeping the staffing and contractual-language providers small and consistent.
/// </summary>
public static class MatrixTableHelper
{
    public static TableData Export(
        string rowHeader,
        IReadOnlyList<ReferenceItemDto> rowItems,
        IReadOnlyList<ReferenceItemDto> colItems,
        IReadOnlyList<MatrixCellValue> cells)
    {
        var headers = new List<string> { rowHeader };
        headers.AddRange(colItems.Select(column => column.Name));

        var valueByRowAndColumn = cells.ToDictionary(cell => (cell.RowRefId, cell.ColRefId), cell => cell.Value);

        var rows = new List<IReadOnlyList<string?>>();
        foreach (var rowItem in rowItems)
        {
            var rowCells = new List<string?> { rowItem.Name };
            foreach (var colItem in colItems)
            {
                rowCells.Add(
                    valueByRowAndColumn.TryGetValue((rowItem.Id, colItem.Id), out var value)
                        ? CellParsers.FormatNumber((double?)value)
                        : string.Empty);
            }
            rows.Add(rowCells);
        }

        return new TableData(headers, rows);
    }

    public static TableData Template(string rowHeader, IReadOnlyList<ReferenceItemDto> colItems)
    {
        var headers = new List<string> { rowHeader };
        headers.AddRange(colItems.Select(column => column.Name));
        return TableData.TemplateOf(headers);
    }

    public static List<MatrixCellValue> Import(TableData data, ReferenceLookup rowLookup, ReferenceLookup columnLookup)
    {
        // Map each data column (after the first label column) to a column reference id by its header name.
        var columnIdByIndex = new Dictionary<int, int>();
        for (var index = 1; index < data.Headers.Count; index++)
        {
            var columnId = columnLookup.IdOf(data.Headers[index]);
            if (columnId is not null)
            {
                columnIdByIndex[index] = columnId.Value;
            }
        }

        var cells = new List<MatrixCellValue>();
        foreach (var row in data.Rows)
        {
            var rowId = rowLookup.IdOf(row.At(0));
            if (rowId is null)
            {
                continue;
            }

            foreach (var (columnIndex, columnId) in columnIdByIndex)
            {
                var value = CellParsers.Double(row.At(columnIndex));
                if (value is not null)
                {
                    cells.Add(new MatrixCellValue(rowId.Value, columnId, value.Value));
                }
            }
        }

        return cells;
    }
}
