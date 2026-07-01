using System.Net.Http;
using System.Text;
using System.Text.Json;
using AidDashboard.Application.Chat;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

namespace AidDashboard.Infrastructure.Services;

/// <summary>
/// Azure OpenAI chat assistant (gpt-5.3-chat, api-version 2024-10-21 — the same call shape the
/// reference FFI_MVP app used for that model). Instead of stuffing all data into the prompt, the
/// model is given only a compact schema description and retrieves what it needs by emitting
/// read-only SQL queries: each turn it replies with either {"action":"query"} or
/// {"action":"answer"}. Queries run against whitelisted read-only views (see
/// <see cref="AiDatabaseViews"/> / <see cref="AiQueryExecutor"/>), the rows are fed back, and the
/// loop continues until the model answers or the step budget is reached.
/// </summary>
public class ChatService : IChatService
{
    private const int MaxSteps = 6;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    private static string? _cachedSchema;

    public ChatService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    private string? Setting(string key, string envVar) =>
        _configuration[$"AzureOpenAI:{key}"] is { Length: > 0 } value ? value : Environment.GetEnvironmentVariable(envVar);

    public async Task<ChatResponseDto> AskAsync(IReadOnlyList<ChatMessageDto> messages, CancellationToken cancellationToken = default)
    {
        var endpoint = Setting("Endpoint", "AZURE_OPENAI_ENDPOINT");
        var apiKey = Setting("ApiKey", "AZURE_OPENAI_API_KEY");
        var deployment = Setting("Deployment", "AZURE_OPENAI_DEPLOYMENT") ?? "gpt-5.3-chat";
        var apiVersion = Setting("ApiVersion", "AZURE_OPENAI_API_VERSION") ?? "2024-10-21";
        var temperature = double.TryParse(Setting("Temperature", "LLM_TEMPERATURE"), out var parsed) ? parsed : 1.0;

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            return new ChatResponseDto
            {
                Configured = false,
                Reply = "The AI assistant isn't connected yet. An administrator can enable it by setting the "
                    + "Azure OpenAI endpoint and key (AzureOpenAI:Endpoint / AzureOpenAI:ApiKey, deployment "
                    + $"\"{deployment}\", api-version {apiVersion}).",
            };
        }

        var connectionString = _configuration.GetConnectionString("Default") ?? "Data Source=aid-dashboard.db";
        var executor = new AiQueryExecutor(connectionString, AiDatabaseViews.ViewNames);
        var url = $"{endpoint.TrimEnd('/')}/openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";

        // Seed the conversation: our system prompt (protocol + schema) then the client's turns.
        var conversation = new List<object> { new { role = "system", content = BuildSystemPrompt() } };
        foreach (var message in messages)
        {
            conversation.Add(new { role = message.Role == "assistant" ? "assistant" : "user", content = message.Content });
        }

        try
        {
            for (var step = 0; step < MaxSteps; step++)
            {
                var content = await CallModelAsync(url, apiKey, temperature, conversation, cancellationToken);
                var (action, sql, answer) = ParseAction(content);

                if (action == "answer")
                {
                    return new ChatResponseDto { Configured = true, Reply = string.IsNullOrWhiteSpace(answer) ? "(no answer)" : answer!.Trim() };
                }

                if (action == "query" && !string.IsNullOrWhiteSpace(sql))
                {
                    conversation.Add(new { role = "assistant", content });
                    var rejection = executor.Validate(sql!);
                    if (rejection != null)
                    {
                        conversation.Add(new { role = "user", content = $"QUERY REJECTED: {rejection}" });
                        continue;
                    }

                    try
                    {
                        var result = await executor.ExecuteAsync(sql!, cancellationToken);
                        conversation.Add(new { role = "user", content = "QUERY RESULT:\n" + AiQueryExecutor.Render(result) });
                    }
                    catch (Exception queryError)
                    {
                        conversation.Add(new { role = "user", content = $"QUERY ERROR: {queryError.Message}. Fix the SQL and try again." });
                    }
                    continue;
                }

                // Unexpected / malformed reply — nudge the model back to the protocol.
                conversation.Add(new { role = "assistant", content });
                conversation.Add(new { role = "user", content = "Reply with a single JSON object: {\"action\":\"query\",\"sql\":\"…\"} or {\"action\":\"answer\",\"answer\":\"…\"}." });
            }

            // Out of steps: ask once for a best-effort final answer.
            conversation.Add(new { role = "user", content = "Provide your best final answer now as {\"action\":\"answer\",\"answer\":\"…\"}." });
            var finalContent = await CallModelAsync(url, apiKey, temperature, conversation, cancellationToken);
            var (_, _, finalAnswer) = ParseAction(finalContent);
            return new ChatResponseDto
            {
                Configured = true,
                Reply = string.IsNullOrWhiteSpace(finalAnswer)
                    ? "I couldn't gather enough data to answer that. Could you rephrase or narrow it down?"
                    : finalAnswer!.Trim(),
            };
        }
        catch (Exception exception)
        {
            return new ChatResponseDto { Configured = true, Reply = $"The assistant is temporarily unavailable: {exception.Message}" };
        }
    }

    /// <summary>Posts the conversation to Azure OpenAI and returns the assistant message content.</summary>
    private async Task<string> CallModelAsync(string url, string apiKey, double temperature, List<object> conversation, CancellationToken cancellationToken)
    {
        var payload = new { messages = conversation, temperature, response_format = new { type = "json_object" } };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("api-key", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(60);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Azure OpenAI returned {(int)response.StatusCode}.");
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
    }

    /// <summary>Extracts (action, sql, answer) from the model's JSON reply, tolerating code fences.</summary>
    private static (string? Action, string? Sql, string? Answer) ParseAction(string content)
    {
        var text = content.Trim();
        if (text.StartsWith("```"))
        {
            text = text.Trim('`');
            if (text.StartsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                text = text[4..];
            }
            text = text.Trim();
        }

        try
        {
            using var document = JsonDocument.Parse(text);
            var root = document.RootElement;
            var action = root.TryGetProperty("action", out var actionElement) ? actionElement.GetString() : null;
            var sql = root.TryGetProperty("sql", out var sqlElement) ? sqlElement.GetString() : null;
            string? answer = null;
            if (root.TryGetProperty("answer", out var answerElement))
            {
                answer = answerElement.GetString();
            }
            else if (root.TryGetProperty("text", out var textElement))
            {
                answer = textElement.GetString();
            }
            return (action, sql, answer);
        }
        catch (JsonException)
        {
            return (null, null, null);
        }
    }

    /// <summary>System prompt: the query/answer protocol plus the schema description (cached).</summary>
    private static string BuildSystemPrompt()
    {
        _cachedSchema ??= LoadSchema();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        return
            "You are the AID Dashboard assistant. You answer questions about the app's data.\n" +
            "You do NOT have the data directly — you retrieve it by querying read-only SQL views.\n\n" +
            "PROTOCOL — every reply MUST be a single JSON object, exactly one of:\n" +
            "  {\"action\":\"query\",\"sql\":\"<one read-only SQLite SELECT over the views below>\"}\n" +
            "  {\"action\":\"answer\",\"answer\":\"<your final answer for the user>\"}\n" +
            "Use \"query\" to fetch what you need (filter by the account or key mentioned; add LIMIT for broad questions). " +
            "After each query I send the rows back, then you may query again or answer. " +
            "Use \"answer\" only once you have the data. If the data doesn't contain the answer, say so. " +
            "Keep answers concise and factual.\n" +
            $"Today's date is {today}.\n\n" +
            "SCHEMA:\n" + _cachedSchema;
    }

    private static string LoadSchema()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "AiAssistant", "schema.md");
        return File.Exists(path) ? File.ReadAllText(path) : "(schema description unavailable)";
    }
}
