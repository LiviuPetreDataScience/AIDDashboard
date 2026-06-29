using System.Globalization;
using AidDashboard.Application.Reference;

namespace AidDashboard.Infrastructure.ImportExport;

/// <summary>Bi-directional lookup between a reference list's names and ids, for export/import mapping.</summary>
public sealed class ReferenceLookup
{
    private readonly Dictionary<string, int> _idByName;
    private readonly Dictionary<int, string> _nameById;

    public ReferenceLookup(IEnumerable<ReferenceItemDto> items)
    {
        _nameById = new Dictionary<int, string>();
        _idByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            _nameById[item.Id] = item.Name;
            _idByName[item.Name.Trim()] = item.Id;
        }
    }

    public string? NameOf(int? id) => id is int value && _nameById.TryGetValue(value, out var name) ? name : null;

    public int? IdOf(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        return _idByName.TryGetValue(name.Trim(), out var id) ? id : null;
    }
}

/// <summary>Convenience access to a row cell by column index, tolerant of short rows.</summary>
public static class RowExtensions
{
    public static string? At(this IReadOnlyList<string?> row, int index) =>
        index >= 0 && index < row.Count ? row[index] : null;
}

/// <summary>Tolerant parsers for converting imported string cells back to typed values.</summary>
public static class CellParsers
{
    public static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static int? Int(string? value) =>
        int.TryParse(value?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;

    public static long? Long(string? value) =>
        long.TryParse(value?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;

    public static double? Double(string? value) =>
        double.TryParse(value?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;

    public static decimal? Decimal(string? value) =>
        decimal.TryParse(value?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;

    public static DateOnly? Date(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        return DateOnly.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
            ? result
            : null;
    }

    /// <summary>Parses Yes/No/true/false text to a nullable boolean.</summary>
    public static bool? YesNo(string? value)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        if (text.Equals("Yes", StringComparison.OrdinalIgnoreCase) || text.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (text.Equals("No", StringComparison.OrdinalIgnoreCase) || text.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return null;
    }

    public static bool Bool(string? value) => YesNo(value) ?? false;

    public static string FormatDate(DateOnly? date) => date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    public static string FormatYesNo(bool? value) => value == null ? string.Empty : value.Value ? "Yes" : "No";

    public static string FormatNumber(double? value) => value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

    public static string FormatNumber(decimal? value) => value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

    public static string FormatNumber(long? value) => value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

    public static string FormatNumber(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
}
