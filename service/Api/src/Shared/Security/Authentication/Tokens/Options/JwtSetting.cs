namespace Shared.Security.Authentication.Tokens.Options;

/// <summary>
/// Configuration options for JWT authentication.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// The configuration section name for JWT settings.
    /// </summary>
    public const string SectionName = "Authentication:Jwt";

    /// <summary>
    /// Gets or sets the secret key used to sign JWT tokens.
    /// </summary>
    public string Secret { get; set; } = null!;

    /// <summary>
    /// Gets or sets the JWT issuer.
    /// </summary>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the JWT audience.
    /// </summary>
    public string Audience { get; set; } = null!;

    /// <summary>
    /// Gets or sets the access token expiration time in minutes.
    /// </summary>
    public int AccessTokenExpirationInMinutes { get; set; }

    /// <summary>
    /// Gets or sets the refresh token expiration time in days.
    /// </summary>
    public int RefreshTokenExpirationInDays { get; set; }

    /// <summary>
    /// Gets or sets the JWT algorithm (e.g., HS256, RS256, ES256).
    /// </summary>
    public string Algorithm { get; set; } = "HS256";

    /// <summary>
    /// Gets or sets the token security options.
    /// </summary>
    public TokenSecurityOptions TokenSecurity { get; set; } = new();
}

/// <summary>
/// Configuration options for token security features.
/// </summary>
public sealed class TokenSecurityOptions
{
    /// <summary>
    /// The configuration section name for token security options.
    /// </summary>
    public const string SectionName = "TokenSecurity";

    /// <summary>
    /// Gets or sets whether token rotation is enabled.
    /// </summary>
    public bool RotationEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether token reuse detection is enabled.
    /// </summary>
    public bool ReuseDetectionEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether sliding expiration is enabled.
    /// </summary>
    public bool SlidingExpirationEnabled { get; set; }

    /// <summary>
    /// Gets or sets the maximum age of a token in days before it must be rotated.
    /// </summary>
    public int MaxTokenAgeDays { get; set; } = 30;
}
