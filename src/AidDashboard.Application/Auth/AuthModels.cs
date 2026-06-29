using AidDashboard.Domain.Identity;

namespace AidDashboard.Application.Auth;

/// <summary>Result of a successful authentication, independent of the identity provider used.</summary>
public record AuthenticatedUser(int Id, string Username, string? DisplayName, string? Email, UserRole Role, string Provider);

public record LoginRequest(string Username, string Password);

public record UserDto(int Id, string Username, string? DisplayName, string? Email, UserRole Role, bool IsActive, bool MustChangePassword);

public record LoginResult(string Token, DateTime ExpiresAtUtc, UserDto User);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
