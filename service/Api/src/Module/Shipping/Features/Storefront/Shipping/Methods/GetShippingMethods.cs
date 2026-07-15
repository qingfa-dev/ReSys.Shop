using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Storefront.Shipping.Methods;
/// <summary>Retrieves all shipping methods available to storefront users.</summary>
public static partial class GetShippingMethods
{
    public sealed record Query : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext, ILogger<QueryHandler> logger)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads active, non-deleted shipping methods and returns them as a list.</summary>
        /// <param name="request">The empty query.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the list of available shipping methods.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=none, post=list of available shipping methods returned
            _ = logger;
            // Load: All available shipping methods.
            var methods = await dbContext.Set<ShippingMethod>()
                .Where(x => x.AvailableToUsers && !x.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map: Return list of available shipping methods.
            // EXCEPTION: no domain entity — maps from domain ShippingMethod entities to DTOs
            return new Response(methods.Select(m => new ShippingMethodDto(m.Id, m.Name, m.AdminName, m.Code, m.CalculatorType, m.Position)).ToList());
        }
    }
}