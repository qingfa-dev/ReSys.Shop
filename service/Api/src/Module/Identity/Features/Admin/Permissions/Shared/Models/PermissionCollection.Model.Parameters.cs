namespace Module.Identity.Features.Shared.Admin.Permissions.Shared.Models;

public abstract record PermissionCollectionParameters
{
    public IEnumerable<string> Permissions { get; init; } = [];
}
