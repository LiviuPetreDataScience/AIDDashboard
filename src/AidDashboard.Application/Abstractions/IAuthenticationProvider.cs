using AidDashboard.Application.Auth;

namespace AidDashboard.Application.Abstractions;

/// <summary>
/// Pluggable authentication provider. The local username/password provider is used today;
/// an Entra ID / Active Directory provider can be added later by implementing this interface
/// without changing the rest of the application.
/// </summary>
public interface IAuthenticationProvider
{
    /// <summary>Provider identifier, e.g. "Local" or "EntraId".</summary>
    string Name { get; }

    /// <summary>Validates credentials and returns the authenticated user, or null if invalid.</summary>
    Task<AuthenticatedUser?> AuthenticateAsync(string username, string password, CancellationToken ct = default);
}
