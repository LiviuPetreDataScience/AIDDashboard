using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace AidDashboard.Infrastructure.Services;

/// <summary>Result of running an assistant-supplied query.</summary>
public sealed class QueryResult
{
    public IReadOnlyList<string> Columns { get; init; } = Array.Empty<string>();
    public IReadOnlyList<IReadOnlyList<object?>> Rows { get; init; } = Array.Empty<IReadOnlyList<object?>>();
    public bool Truncated { get; init; }
}

/// <summary>
/// Validates and runs the read-only SELECT queries the AI assistant produces. Safety is layered:
/// the query must be a single SELECT, may only reference the whitelisted views, must contain no
/// data-modifying keywords, and is executed on a read-only SQLite connection with a row cap.
/// </summary>
public sealed class AiQueryExecutor
{
    public const int RowCap = 200;

    private static readonly string[] ForbiddenKeywords =
    {
        "insert", "update", "delete", "drop", "alter", "create", "attach", "detach",
        "pragma", "vacuum", "replace", "reindex", "truncate", "into", "trigger",
    };

    private readonly string _readOnlyConnectionString;
    private readonly IReadOnlySet<string> _allowedViews;

    public AiQueryExecutor(string baseConnectionString, IReadOnlySet<string> allowedViews)
    {
        // Force a read-only connection regardless of the base settings (defence in depth).
        var builder = new SqliteConnectionStringBuilder(baseConnectionString) { Mode = SqliteOpenMode.ReadOnly };
        _readOnlyConnectionString = builder.ToString();
        _allowedViews = allowedViews;
    }

    /// <summary>Returns null if the SQL is allowed, otherwise a human-readable reason it was rejected.</summary>
    public string? Validate(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return "The query was empty.";
        }

        var trimmed = sql.Trim().TrimEnd(';').Trim();
        if (trimmed.Contains(';'))
        {
            return "Only a single statement is allowed (no ';').";
        }

        var lower = trimmed.ToLowerInvariant();
        if (!lower.StartsWith("select"))
        {
            return "Only read-only SELECT queries are allowed.";
        }

        foreach (var keyword in ForbiddenKeywords)
        {
            if (Regex.IsMatch(lower, $@"\b{keyword}\b"))
            {
                return $"The keyword '{keyword}' is not allowed. Use a plain read-only SELECT.";
            }
        }

        // Every table/view referenced after FROM/JOIN must be one of the allowed views.
        foreach (Match match in Regex.Matches(trimmed, @"(?i)\b(?:from|join)\s+[""\[]?([a-zA-Z_][a-zA-Z0-9_]*)"))
        {
            var referenced = match.Groups[1].Value;
            if (!_allowedViews.Contains(referenced))
            {
                return $"Table '{referenced}' is not available. You may only query these views: {string.Join(", ", _allowedViews)}.";
            }
        }

        return null;
    }

    public async Task<QueryResult> ExecuteAsync(string sql, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_readOnlyConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA query_only = 1;";
            await pragma.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var columns = new List<string>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            columns.Add(reader.GetName(i));
        }

        var rows = new List<IReadOnlyList<object?>>();
        var truncated = false;
        while (await reader.ReadAsync(cancellationToken))
        {
            if (rows.Count >= RowCap)
            {
                truncated = true;
                break;
            }

            var values = new object?[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++)
            {
                values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            rows.Add(values);
        }

        return new QueryResult { Columns = columns, Rows = rows, Truncated = truncated };
    }

    /// <summary>Compact JSON-ish rendering of a result for feeding back to the model.</summary>
    public static string Render(QueryResult result)
    {
        var builder = new StringBuilder();
        builder.Append("columns: ").Append(string.Join(", ", result.Columns)).Append('\n');
        builder.Append("rows (").Append(result.Rows.Count).Append(result.Truncated ? "+, truncated" : "").Append("):\n");
        foreach (var row in result.Rows)
        {
            builder.Append(System.Text.Json.JsonSerializer.Serialize(row)).Append('\n');
        }
        if (result.Rows.Count == 0)
        {
            builder.Append("(no rows)\n");
        }
        return builder.ToString();
    }
}
