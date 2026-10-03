namespace Module.Shipping.Features.Admin.Shared.Models;

#region Parameters
public abstract record ShippingRateParameters : INamedParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public decimal Cost { get; init; }
    public string? DeliveryRange { get; init; }
    public decimal? MinWeight { get; init; }
    public decimal? MaxWeight { get; init; }
    public decimal? FreeShippingThreshold { get; init; }
    public Guid ShippingMethodId { get; init; }
}
#endregion

#region Request
public record ShippingRateRequest : ShippingRateParameters;

#endregion

#region Response
public record ShippingRateDetailResponse : ShippingRateParameters
{
    public Guid Id { get; init; }
    public decimal FinalPrice { get; init; }
    public bool Selected { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public record ShippingRateListItemResponse : ShippingRateDetailResponse;
#endregion
