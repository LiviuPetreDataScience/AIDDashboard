using AidDashboard.Application.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AidDashboard.Api.Controllers;

/// <summary>The AID Dashboard AI assistant. Available to any authenticated user.</summary>
[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <summary>Answers a question grounded in the current app data.</summary>
    [HttpPost]
    public async Task<ActionResult<ChatResponseDto>> Ask([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
    {
        var answer = await _chatService.AskAsync(request.Messages, cancellationToken);
        return Ok(answer);
    }
}
