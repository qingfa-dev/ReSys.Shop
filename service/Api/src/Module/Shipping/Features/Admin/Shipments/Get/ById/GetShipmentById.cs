using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Shared.Mappings;

namespace Module.Shipping.Features.Admin.Shipments.Get.ById;
/// <summary>Gets a shipment by its ID.</summary>
public static partial class GetShipmentById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext, ILogger<QueryHandler> logger) : IQueryHandler<Query, Response>
    {
        /// <summary>Handles retrieving a shipment by ID.</summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The shipment response.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Contract: pre=query!=null, post=result!=null
            _ = logger;
            // Query: Get shipment by ID.
            var shipment = await dbContext.Set<Shipment>().AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == query.Id, cancellationToken);
            if (shipment is null)
                return ShipmentResult.Errors.NotFound(query.Id);

            // Map: Return shipment details via mapping.
            return Result<Response>.Ok(shipment.MapToDetail<Response>(), "Shipment retrieved.");
        }
    }
}
