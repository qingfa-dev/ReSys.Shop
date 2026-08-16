using Module.Identity.Features.Admin.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Roles.Permissions.Revoke;

/// <summary>
/// Represents the request contract for revoking permissions from a role.
/// </summary>
public record Request : PermissionCollectionParameters;