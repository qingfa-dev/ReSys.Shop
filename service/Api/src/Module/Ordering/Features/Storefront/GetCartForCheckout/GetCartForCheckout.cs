using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.GetCartForCheckout;

public sealed class GetCartForCheckoutQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetCartForCheckoutQuery, GetCartForCheckoutResponse>
{
    public async Task<Result<GetCartForCheckoutResponse>> Handle(
        GetCartForCheckoutQuery query, CancellationToken cancellationToken)
    {
        var cart = await dbContext.Set<Order>()
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(
                x => x.Id == query.CartId && x.Status == OrderStatus.Draft,
                cancellationToken);

        if (cart is null)
            return OrderResult.Errors.NotFound(query.CartId);

        return new GetCartForCheckoutResponse
        {
            State = cart.CheckoutState.ToString(),
            LineItems = cart.LineItems
                .Select(li => new CartLineItem
                {
                    VariantId = li.VariantId,
                    Quantity = li.Quantity
                })
                .ToList(),
            Total = cart.Total,
            Email = cart.Email
        };
    }
}
