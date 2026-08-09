using Shared.Security.Identity.Domain.Permissions;

namespace Module.Identity.Features.Shared.Admin.Permissions.Shared.Models;

public sealed record ResourceGroup
{
    public string ResourceName { get; init; } = default!;
    public IReadOnlyList<PermissionMetadata> Permissions { get; init; } = default!;
    public string? Description { get; init; } = null;
}

public sealed record PermissionGroup
{
    public string Category { get; init; } = default!;
    public IReadOnlyList<ResourceGroup> Resources { get; init; } = default!;
    public string? Description { get; init; } = null;
}