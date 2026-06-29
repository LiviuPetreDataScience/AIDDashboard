namespace AidDashboard.Application.Abstractions;

/// <summary>
/// Provides the identity of the user making the current request.
/// Implemented in the API layer over the HTTP context.
/// </summary>
public interface ICurrentUserAccessor
{
    int? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
