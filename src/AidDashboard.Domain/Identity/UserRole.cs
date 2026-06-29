namespace AidDashboard.Domain.Identity;

/// <summary>
/// Application roles. Admin = full edit + user/reference management. User = read-only.
/// </summary>
public enum UserRole
{
    Admin = 0,
    User = 1
}
