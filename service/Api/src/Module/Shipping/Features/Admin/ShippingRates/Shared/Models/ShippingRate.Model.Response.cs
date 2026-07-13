namespace Module.Shipping.Features.Admin.ShippingRates.Shared.Models;

public class ShippingRateDetailResponse : ShippingRateParameters
{
    public Guid Id { get; init; }
    public decimal FinalPrice { get; init; }
    public bool Selected { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public class ShippingRateListItemResponse : ShippingRateDetailResponse { }