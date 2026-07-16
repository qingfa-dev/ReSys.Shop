using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Storefront.Shipping.Rates;
/// <summary>Lists shipping rates available for storefront checkout.</summary>
public static partial class ListShippingRates
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext, ILogger<PagedQueryHandler> logger)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Loads and paginates shipping rates with full cost and delivery details.</summary>
        /// <param name="request">The query containing paging parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A paged result of shipping rate details.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            _ = logger;
            var parsing = request.Parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await dbContext.Set<ShippingRate>()
                .AsNoTracking()
                .ApplyQuerying(parsing.Value)
                // EXCEPTION: no domain entity — maps from domain ShippingRate entity
                .Select(r => new Response(r.Id, r.ShippingMethodId, r.Name, r.Cost, r.FinalPrice, r.DeliveryRange, r.MinWeight, r.MaxWeight, r.FreeShippingThreshold))
                .ToPagedOrAllAsync(parsing.Value, x => x, cancellationToken);

            return pagedResult;
        }
    }
}