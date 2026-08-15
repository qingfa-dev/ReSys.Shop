using Module.Identity.Features.Shared.Admin.Permissions.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Roles.Permissions.Revoke;

/// <summary>
/// Represents the request contract for revoking permissions from a role.
/// </summary>
public record Request : PermissionCollectionParameters;