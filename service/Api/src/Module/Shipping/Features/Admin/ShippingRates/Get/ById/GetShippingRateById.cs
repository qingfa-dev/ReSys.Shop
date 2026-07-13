using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.ShippingRates.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingRates.Get.ById;

/// <summary>Retrieves a shipping rate by its unique identifier.</summary>
public static partial class GetShippingRateById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads a single shipping rate by ID using a no-tracking query.</summary>
        /// <param name="request">The query containing the shipping rate ID.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the shipping rate details or a not-found error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=rate found or NotFound returned
            // Load: Shipping rate by ID
            var rate = await dbContext.Set<ShippingRate>()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (rate is null)
                return ShippingRateResult.Errors.NotFound(request.Id);

            return rate.MapToDetail<Response>();
        }
    }
}