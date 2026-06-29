namespace AidDashboard.Api.Security;

/// <summary>
/// Strongly-typed JWT configuration, bound from the "Jwt" configuration section.
/// The signing key must be supplied via configuration / environment / Key Vault in production.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Token issuer (the API).</summary>
    public string Issuer { get; set; } = "AidDashboard";

    /// <summary>Intended token audience (the SPA).</summary>
    public string Audience { get; set; } = "AidDashboard";

    /// <summary>Symmetric signing key. Must be long and secret in production.</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Access-token lifetime in minutes.</summary>
    public int ExpiryMinutes { get; set; } = 480;
}
