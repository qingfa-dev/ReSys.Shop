using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shared.Mappings;

namespace Module.Shipping.Features.Admin.Shipments.ListForOrder;

/// <summary>Lists shipments for an order.</summary>
public static partial class GetShipmentsForOrder
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Loads all shipments belonging to the requested order.</summary>
        /// <param name="request">The query containing the order identifier.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>The order's shipments, oldest first.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {

            // Contract: pre=request!=null, post=paged result returned
            var parsing = request.Parameters.ParseAll(
                allowedFilterFields: ShipmentConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: ShipmentConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: ShipmentConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return parsing.Errors;


            var shipments = await dbContext.Set<Shipment>()
                .AsNoTracking()
                .Where(s => request.Parameters.OrderId == null || s.OrderId == request.Parameters.OrderId)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, s => s.MapToListItem<Response>(), cancellationToken);

            return shipments;
        }
    }
}
