using AidDashboard.Application.Auth;
using AidDashboard.Domain.Identity;

namespace AidDashboard.Application.Admin;

/// <summary>Payload to create a new application user (admin only).</summary>
public record CreateUserRequest(string Username, string? DisplayName, string? Email, UserRole Role, string Password);

/// <summary>Payload to update an existing user's profile and role (admin only).</summary>
public record UpdateUserRequest(string? DisplayName, string? Email, UserRole Role, bool IsActive);

/// <summary>Payload to reset a user's password (admin only).</summary>
public record ResetPasswordRequest(string NewPassword);

/// <summary>Outcome of a create-user attempt, distinguishing a duplicate username from success.</summary>
public record CreateUserResult(UserDto? User, bool UsernameAlreadyExists);

/// <summary>Administrative user management.</summary>
public interface IUserAdminService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<CreateUserResult> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<bool> ResetPasswordAsync(int userId, ResetPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deactivates a user (kept for audit history rather than hard-deleted).</summary>
    Task<bool> DeactivateAsync(int userId, CancellationToken cancellationToken = default);
}
