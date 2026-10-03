namespace Shared.Security.Authentication.External.Models;

/// <summary>
/// Result DTO returned by an external login provider after successful token validation.
/// </summary>
public sealed record ExternalUserInfo
{
    /// <summary>The OAuth provider key (e.g. "google").</summary>
    public string Provider { get; init; } = default!;

    /// <summary>The unique subject ID from the provider.</summary>
    public string ProviderSubjectId { get; init; } = default!;

    /// <summary>The user's email address from the provider.</summary>
    public string Email { get; init; } = default!;

    /// <summary>The user's first name (or fallback).</summary>
    public string FirstName { get; init; } = default!;

    /// <summary>The user's last name, if provided.</summary>
    public string? LastName { get; init; }
}
