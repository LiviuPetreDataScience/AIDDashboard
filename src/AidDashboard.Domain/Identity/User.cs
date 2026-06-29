using AidDashboard.Domain.Common;

namespace AidDashboard.Domain.Identity;

/// <summary>
/// An application user. Authentication is local (username + password hash) for now,
/// but the auth layer is abstracted so Entra ID / Active Directory can be added later.
/// </summary>
public class User : AuditableEntity
{
    public string Username { get; set; } = default!;
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    /// <summary>When true, the user is forced to change password on next login. Not used at launch.</summary>
    public bool MustChangePassword { get; set; }
    /// <summary>Identity provider that owns this account. "Local" today; "EntraId" later.</summary>
    public string Provider { get; set; } = "Local";
}
