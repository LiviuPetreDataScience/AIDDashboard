using AidDashboard.Application.Abstractions;
using AidDashboard.Application.Auth;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Auth;

/// <summary>Authenticates against locally-stored username/password credentials.</summary>
public class LocalAuthenticationProvider : IAuthenticationProvider
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public LocalAuthenticationProvider(AppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public string Name => "Local";

    public async Task<AuthenticatedUser?> AuthenticateAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var normalized = username.Trim().ToLower();
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == normalized && u.IsActive, ct);

        if (user is null || user.Provider != "Local")
            return null;

        if (!_hasher.Verify(password, user.PasswordHash))
            return null;

        return new AuthenticatedUser(user.Id, user.Username, user.DisplayName, user.Email, user.Role, user.Provider);
    }
}
