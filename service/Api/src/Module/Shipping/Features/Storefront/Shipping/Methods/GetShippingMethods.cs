using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Storefront.Shipping.Methods;
/// <summary>Gets available shipping methods for the storefront.</summary>
public static partial class GetShippingMethods
{
    public sealed record Query : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext, ILogger<QueryHandler> logger)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Handles retrieving shipping methods.</summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The shipping methods response.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            _ = logger;
            // Query: Retrieve all available shipping methods.
            var methods = await dbContext.Set<ShippingMethod>()
                .Where(x => x.AvailableToUsers && !x.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map: Return list of shipping methods.
            return new Response
            {
                Methods = methods.Select(m => new ShippingMethodDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    AdminName = m.AdminName,
                    Code = m.Code,
                    CalculatorType = m.CalculatorType,
                    Position = m.Position
                }).ToList()
            };
        }
    }
}
