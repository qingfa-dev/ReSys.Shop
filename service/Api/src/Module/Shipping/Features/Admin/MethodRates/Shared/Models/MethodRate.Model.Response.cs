namespace Module.Shipping.Features.Admin.MethodRates.Shared.Models;

/// <summary>Detail response for a method rate, including audit fields.</summary>
public class MethodRateDetailResponse : MethodRateParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}

/// <summary>List item response for a method rate.</summary>
public class MethodRateListItemResponse : MethodRateDetailResponse;
