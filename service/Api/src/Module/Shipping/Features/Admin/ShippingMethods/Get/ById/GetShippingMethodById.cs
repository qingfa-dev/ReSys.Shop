using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingMethods.Get.ById;
/// <summary>Gets a shipping method by its ID.</summary>
public static partial class GetShippingMethodById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext, ILogger<QueryHandler> logger) : IQueryHandler<Query, Response>
    {
        /// <summary>Handles retrieving a shipping method by ID.</summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The shipping method response.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Contract: pre=query!=null, post=result!=null
            _ = logger;
            // Query: Get shipping method by ID.
            var method = await dbContext.Set<ShippingMethod>().AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == query.Id, cancellationToken);
            if (method is null)
                return ShippingMethodResult.Errors.NotFound;

            // Map: Return method details via mapping.
            return Result<Response>.Ok(method.MapToDetail<Response>(), ShippingMethodResult.Success.Updated);
        }
    }
}
