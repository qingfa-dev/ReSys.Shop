namespace Module.Identity.Features.Admin.Shared.Models;

public abstract record RoleCollectionParameters
{
    public IEnumerable<string> Roles { get; init; } = [];
}
