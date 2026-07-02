namespace Module.Identity.Features.Admin.Roles.Shared.Models;

public abstract class RoleParameter
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
