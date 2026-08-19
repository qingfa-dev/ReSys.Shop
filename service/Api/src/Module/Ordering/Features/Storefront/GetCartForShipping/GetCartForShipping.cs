using Module.Catalog.Domain.Variants;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.GetCartForShipping;

public sealed class GetCartForShippingQueryHandler(
    IApplicationDbContext dbContext)
    : IQueryHandler<GetCartForShippingQuery, CartForShippingResponse>
{
    public async Task<Result<CartForShippingResponse>> Handle(
        GetCartForShippingQuery query, CancellationToken cancellationToken)
    {
        var cart = await dbContext.Set<Order>()
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(
                x => x.Id == query.CartId && x.Status == OrderStatus.Draft,
                cancellationToken);

        if (cart is null)
            return OrderResult.Errors.NotFound(query.CartId);

        var variantIds = cart.LineItems.Select(li => li.VariantId).Distinct().ToList();
        var variantWeights = await dbContext.Set<Variant>()
            .Where(v => variantIds.Contains(v.Id))
            .Select(v => new { v.Id, v.Weight })
            .ToListAsync(cancellationToken);
        var weightMap = variantWeights.ToDictionary(v => v.Id, v => v.Weight ?? 0m);
        var totalWeight = cart.CalculateTotalWeight(weightMap);

        return new CartForShippingResponse
        {
            TotalWeight = totalWeight,
            TotalValue = cart.Total,
            ShipAddressId = cart.ShipAddressId,
            Currency = cart.Currency
        };
    }
}
