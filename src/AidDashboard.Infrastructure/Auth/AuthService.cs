using AidDashboard.Application.Abstractions;
using AidDashboard.Application.Auth;
using AidDashboard.Domain.Identity;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly IAuthenticationProvider _provider;
    private readonly ITokenService _tokens;
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AuthService(IAuthenticationProvider provider, ITokenService tokens, AppDbContext db, IPasswordHasher hasher)
    {
        _provider = provider;
        _tokens = tokens;
        _db = db;
        _hasher = hasher;
    }

    public async Task<LoginResult?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var auth = await _provider.AuthenticateAsync(request.Username, request.Password, ct);
        if (auth is null)
            return null;

        var (token, expiresAt) = _tokens.CreateToken(auth);
        var user = await _db.Users.FirstAsync(u => u.Id == auth.Id, ct);
        return new LoginResult(token, expiresAt, ToDto(user));
    }

    public async Task<UserDto?> GetByIdAsync(int userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user is null ? null : ToDto(user);
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null || user.Provider != "Local")
            return false;

        if (!_hasher.Verify(request.CurrentPassword, user.PasswordHash))
            return false;

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return false;

        user.PasswordHash = _hasher.Hash(request.NewPassword);
        user.MustChangePassword = false;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static UserDto ToDto(User u) =>
        new(u.Id, u.Username, u.DisplayName, u.Email, u.Role, u.IsActive, u.MustChangePassword);
}
