using System.Security.Claims;
using AidDashboard.Application.Abstractions;
using AidDashboard.Domain.Identity;

namespace AidDashboard.Api.Security;

/// <summary>
/// Reads the identity of the current request's authenticated user from the HTTP context.
/// Used by the persistence layer to stamp audit fields and by services for authorization.
/// </summary>
public class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public int? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName => Principal?.FindFirstValue(ClaimTypes.Name);

    public bool IsAdmin => Principal?.IsInRole(nameof(UserRole.Admin)) ?? false;
}
