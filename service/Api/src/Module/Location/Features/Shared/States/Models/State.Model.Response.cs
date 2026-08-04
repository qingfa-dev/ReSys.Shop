namespace Module.Location.Features.Shared.States.Models;

public record StateDetailResponse : StateParameters, IResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public record StateListResponse : StateParameters, IResponse
{
    public Guid Id { get; init; }
    public string? CountryName { get; init; }
}