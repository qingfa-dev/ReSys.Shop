namespace Module.Profile.Features.Admin.Profiles.Shared.Models;

public abstract record ProfileDetailResponse : ProfileParameters
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public bool PhoneNumberConfirmed { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}
