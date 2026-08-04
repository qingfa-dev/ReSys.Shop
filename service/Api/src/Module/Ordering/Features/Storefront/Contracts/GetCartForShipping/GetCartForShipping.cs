using Module.Ordering.Domain.Orders;

using Shared.Application.Contracts.Catalog;
using Shared.Application.Contracts.Ordering;

namespace Module.Ordering.Features.Storefront.Contracts.GetCartForShipping;

public sealed class GetCartForShippingQueryHandler(
    IApplicationDbContext dbContext,
    ISender sender)
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
        var weightsResult = await sender.Send(new GetVariantWeightsQuery(variantIds), cancellationToken);
        if (weightsResult.IsFailure)
            return weightsResult.Errors;

        var weightMap = weightsResult.Value.ToDictionary(x => x.Key, x => x.Value);
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
