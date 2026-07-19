namespace Module.Identity.Features.Admin.Roles.Shared.Models;

public record RoleDetailResponse : RoleParameter, IResponse
{
    public Guid Id { get; init; }
    public bool IsSystem { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public record RoleListResponse : RoleParameter, IResponse
{
    public Guid Id { get; init; }
    public bool IsSystem { get; init; }
}