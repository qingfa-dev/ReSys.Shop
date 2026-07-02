namespace Shared.Security.Authentication.External.Models;

/// <summary>
/// Result DTO returned by an external login provider after successful token validation.
/// </summary>
/// <param name="Provider">The OAuth provider key (e.g. "google").</param>
/// <param name="ProviderSubjectId">The unique subject ID from the provider.</param>
/// <param name="Email">The user's email address from the provider.</param>
/// <param name="FirstName">The user's first name (or fallback).</param>
/// <param name="LastName">The user's last name, if provided.</param>
public sealed record ExternalUserInfo(string Provider, string ProviderSubjectId, string Email, string FirstName, string? LastName);
