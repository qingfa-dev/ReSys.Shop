using BuildingBlocks.Querying.Extensions;
using BuildingBlocks.Querying.Models;

using Microsoft.EntityFrameworkCore;
using Module.Shipping.Domain.Shipments;

namespace Module.Ordering.Features.Admin.Orders.Shipments.Get;
    /// <summary>Handles GetOrderShipments feature.</summary>
    public static partial class GetOrderShipments
{
    public sealed record Query(Guid OrderId, QueryingParameters Parameters) : IPagedQuery<Response>;
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext) : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles the query.</summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the query.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var query = dbContext.Set<Shipment>().AsNoTracking()
                .Where(s => s.OrderId == request.OrderId)
                .OrderBy(s => s.CreatedAtUtc)
                .ApplyQueryOptions(parameters);

            var pagedResult = await query
                .Select(s => new Response
                {
                    Id = s.Id, Number = s.Number, State = s.State, Tracking = s.Tracking,
                    Cost = s.Cost, ShippingMethodId = s.ShippingMethodId,
                    StockLocationId = s.StockLocationId, ShippedAtUtc = s.ShippedAtUtc,
                    CreatedAtUtc = s.CreatedAtUtc
                })
                .ToPagedOrAllAsync(x => x, parameters, cancellationToken);

            return pagedResult;
        }
    }
}
