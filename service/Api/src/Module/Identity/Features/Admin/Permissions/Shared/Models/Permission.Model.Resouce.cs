namespace Module.Identity.Features.Shared.Admin.Permissions.Shared.Models;

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