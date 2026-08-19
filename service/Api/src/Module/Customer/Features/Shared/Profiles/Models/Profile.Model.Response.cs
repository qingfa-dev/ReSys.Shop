namespace Module.Customer.Features.Shared.Profiles.Models;

public abstract record ProfileListItemResponse : ProfileParameters
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public abstract record ProfileDetailResponse : ProfileListItemResponse
{
    public bool EmailConfirmed { get; init; }
    public bool PhoneNumberConfirmed { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}
