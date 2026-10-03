namespace Module.Ordering.Features.Storefront.GetCartForShipping;

public sealed record GetCartForShippingQuery : IQuery<CartForShippingResponse>
{
    public Guid CartId { get; init; }
}
