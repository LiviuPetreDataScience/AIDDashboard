using AidDashboard.Application.Auth;

namespace AidDashboard.Application.Abstractions;

public interface IAuthService
{
    Task<LoginResult?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(int userId, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default);
}
