namespace Module.Identity.Features.Admin.Roles.Shared.Models;

public abstract record RoleParameter : INamedParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Presentation { get; init; }
}