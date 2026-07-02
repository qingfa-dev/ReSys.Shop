using Microsoft.AspNetCore.Authorization;

namespace Shared.Security.Authorization.Requirements;

/// <summary>
/// Represents a requirement that the current user must hold a specific
/// permission claim (either granted via their role or directly on their account).
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="PermissionRequirement"/>.
/// </remarks>
/// <param name="permission">The permission name required.</param>
public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the permission name required for authorization.
    /// </summary>
    public string Permission { get; } = permission;
}
