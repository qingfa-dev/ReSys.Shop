namespace Module.Identity.Features.Admin.Roles.Permissions.Revoke;

/// <summary>
/// Represents the request contract for revoking permissions from a role.
/// </summary>
public record Request
{
    /// <summary>
    /// Gets or initializes the collection of permission identifiers to be revoked from the role.
    /// </summary>
    public IEnumerable<string> Permissions { get; init; } = [];
}