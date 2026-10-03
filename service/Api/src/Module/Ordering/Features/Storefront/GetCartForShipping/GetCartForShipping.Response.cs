namespace Module.Ordering.Features.Storefront.GetCartForShipping;

public sealed record CartForShippingResponse
{
    public decimal TotalWeight { get; init; }
    public decimal TotalValue { get; init; }
    public Guid? ShipAddressId { get; init; }
    public string Currency { get; init; } = default!;
}
