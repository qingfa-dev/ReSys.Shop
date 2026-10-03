namespace Module.Location.Features.Shared.Countries.Models;

public record CountryDetailResponse : CountryParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public record CountryListItemResponse : CountryParameters
{
    public Guid Id { get; init; }
}