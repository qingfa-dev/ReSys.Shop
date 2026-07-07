namespace Module.Shipping.Features.Storefront.Shared.Models;

public class ShippingMethodDetailResponse : ShippingMethodParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public class ShippingMethodListItemResponse : ShippingMethodParameters
{
    public Guid Id { get; init; }
}
