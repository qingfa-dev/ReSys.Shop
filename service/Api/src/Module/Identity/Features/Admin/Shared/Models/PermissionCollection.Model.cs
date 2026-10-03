namespace Module.Identity.Features.Admin.Shared.Models;

public abstract record PermissionCollectionParameters
{
    public IEnumerable<string> Permissions { get; init; } = [];
}
