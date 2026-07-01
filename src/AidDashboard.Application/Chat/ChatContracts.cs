namespace AidDashboard.Application.Chat;

/// <summary>One turn in the assistant conversation. Role is "user" or "assistant".</summary>
public class ChatMessageDto
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}

/// <summary>The conversation so far, sent by the chat widget. The service prepends its own system prompt.</summary>
public class ChatRequestDto
{
    public List<ChatMessageDto> Messages { get; set; } = new();
}

/// <summary>The assistant's answer. <see cref="Configured"/> is false when no Azure OpenAI settings are present.</summary>
public class ChatResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public bool Configured { get; set; }
}

/// <summary>
/// Answers questions about the app's data using Azure OpenAI (gpt-5.3-chat). The service grounds
/// each answer in a summary of the current app data assembled from the database.
/// </summary>
public interface IChatService
{
    Task<ChatResponseDto> AskAsync(IReadOnlyList<ChatMessageDto> messages, CancellationToken cancellationToken = default);
}
