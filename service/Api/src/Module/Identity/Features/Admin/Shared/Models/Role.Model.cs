namespace Module.Identity.Features.Admin.Shared.Models;

public abstract record RoleParameter
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record RoleRequest : RoleParameter;

public record RoleDetailResponse : RoleParameter
{
    public Guid Id { get; init; }
    public bool IsSystem { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public record RoleListResponse : RoleParameter
{
    public Guid Id { get; init; }
    public bool IsSystem { get; init; }
}
