using Shared.Security.Identity.Domain.Permissions;

namespace Module.Identity.Features.Admin.Permissions.Shared.Models;

public sealed record ResourceGroup(string ResourceName, IReadOnlyList<PermissionMetadata> Permissions, string? Description = null);

public sealed record PermissionGroup(string Category, IReadOnlyList<ResourceGroup> Resources, string? Description = null);
