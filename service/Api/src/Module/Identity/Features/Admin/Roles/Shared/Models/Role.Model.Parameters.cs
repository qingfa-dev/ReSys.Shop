namespace Module.Identity.Features.Shared.Admin.Roles.Shared.Models;

public abstract record RoleParameter
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}