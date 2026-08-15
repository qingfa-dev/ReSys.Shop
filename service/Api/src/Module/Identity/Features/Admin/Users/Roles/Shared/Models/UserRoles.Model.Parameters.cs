namespace Module.Identity.Features.Shared.Admin.Users.Roles.Shared.Models;

public abstract record RoleCollectionParameters
{
    public IEnumerable<string> Roles { get; init; } = [];
}
