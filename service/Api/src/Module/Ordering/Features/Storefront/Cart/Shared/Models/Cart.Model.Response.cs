namespace Module.Ordering.Features.Storefront.Cart.Shared.Models;

public class CartDetailResponse : CartParameters
{
    public Guid Id { get; init; }
}

public class CartListItemResponse : CartParameters
{
    public Guid Id { get; init; }
}
