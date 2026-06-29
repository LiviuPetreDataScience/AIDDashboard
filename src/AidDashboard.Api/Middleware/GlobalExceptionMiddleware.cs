using System.Text.Json;

namespace AidDashboard.Api.Middleware;

/// <summary>
/// Catches any unhandled exception, logs it, and returns a consistent JSON problem response
/// so the API never leaks stack traces and never crashes the request pipeline.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteErrorResponseAsync(context, exception);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Only expose exception detail outside production to avoid leaking internals.
        var problem = new
        {
            type = "about:blank",
            title = "An unexpected error occurred.",
            status = StatusCodes.Status500InternalServerError,
            detail = _environment.IsDevelopment() ? exception.Message : "Please contact support if the problem persists.",
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
