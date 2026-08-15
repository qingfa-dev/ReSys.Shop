namespace Module.Identity.Features.Shared.Admin.Permissions.Shared.Models;

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