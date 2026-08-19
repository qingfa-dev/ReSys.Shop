namespace Module.Ordering.Features.Storefront.GetCartForCheckout;

public sealed record GetCartForCheckoutQuery : IQuery<GetCartForCheckoutResponse>
{
    public Guid CartId { get; init; }
}
