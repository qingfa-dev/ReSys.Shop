using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Storefront.Shipping.Rates;
/// <summary>Lists available shipping rates for the storefront.</summary>
public static partial class ListShippingRates
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext, ILogger<PagedQueryHandler> logger)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles listing shipping rates.</summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of shipping rates.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            _ = logger;
            var parsing = request.Parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await dbContext.Set<ShippingRate>()
                .AsNoTracking()
                .ApplyQuerying(parsing.Value)
                .Select(r => new Response
                {
                    Id = r.Id,
                    ShippingMethodId = r.ShippingMethodId,
                    Name = r.Name,
                    Cost = r.Cost,
                    FinalPrice = r.FinalPrice,
                    DeliveryRange = r.DeliveryRange,
                    MinWeight = r.MinWeight,
                    MaxWeight = r.MaxWeight,
                    FreeShippingThreshold = r.FreeShippingThreshold
                })
                .ToPagedOrAllAsync(parsing.Value, x => x, cancellationToken);

            return pagedResult;
        }
    }
}
