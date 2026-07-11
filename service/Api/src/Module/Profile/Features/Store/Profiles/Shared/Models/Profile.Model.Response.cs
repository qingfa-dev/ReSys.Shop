namespace Module.Profile.Features.Store.Profiles.Shared.Models;

public abstract class ProfileDetailResponse : ProfileParameter
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public bool PhoneNumberConfirmed { get; init; }

    // Audit
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
}

public abstract class ProfileListItemResponse : ProfileParameter
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
}