namespace Module.Identity.Features.Admin.Users.Shared.Models;

public record UserDetailResponse : UserParameter, IResponse
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}

public record UserListResponse : UserParameter, IResponse
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}