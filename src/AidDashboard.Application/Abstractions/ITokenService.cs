using AidDashboard.Application.Auth;

namespace AidDashboard.Application.Abstractions;

/// <summary>Issues access tokens for authenticated users.</summary>
public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(AuthenticatedUser user);
}
