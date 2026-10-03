using Shared.Security.Identity.Domain.Permissions;

namespace Module.Identity.Features.Admin.Shared.Models;

public abstract record CategoryGroupListItemResponse<TResource>
{
    public string Category { get; init; } = default!;
    public string? Description { get; init; }
    public List<TResource> Resources { get; init; } = [];
}

public abstract record CategoryGroupListResponse<TCategory, TResource>
    where TCategory : CategoryGroupListItemResponse<TResource>
{
    public List<TCategory> Categories { get; set; } = [];
}

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

public abstract record ResourceGroupListItemResponse<TPermission>
    where TPermission : PermissionResponse
{
    public string Resource { get; init; } = default!;
    public string? Description { get; init; }
    public List<TPermission> Permissions { get; init; } = [];
}

public abstract record ResourceGroupListResponse<TResource, TPermission>
    where TResource : ResourceGroupListItemResponse<TPermission>
    where TPermission : PermissionResponse
{
    public List<TResource> Resources { get; init; } = [];
}

public abstract record PermissionResponse
{
    public string Identifier { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public string Action { get; init; } = default!;
}

public abstract record PermissionAssignmentItemResponse : PermissionResponse
{
    public bool IsAssigned { get; init; }
}
