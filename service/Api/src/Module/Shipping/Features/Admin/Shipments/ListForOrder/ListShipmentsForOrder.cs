using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Shared;

namespace Module.Shipping.Features.Admin.Shipments.ListForOrder;

/// <summary>Lists shipments for an order.</summary>
public static partial class ListShipmentsForOrder
{
    public sealed record Query(Guid OrderId) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads all shipments belonging to the requested order.</summary>
        /// <param name="request">The query containing the order identifier.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>The order's shipments, oldest first.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var shipments = await dbContext.Set<Shipment>()
                .AsNoTracking()
                .Where(s => s.OrderId == request.OrderId)
                .OrderBy(s => s.CreatedAtUtc)
                .Select(s => new ShipmentListItem
                {
                    Id = s.Id,
                    OrderId = s.OrderId,
                    ShippingMethodId = s.ShippingMethodId,
                    TrackingNumber = s.TrackingNumber,
                    Status = s.Status,
                    ShippedAtUtc = s.ShippedAtUtc,
                    DeliveredAtUtc = s.DeliveredAtUtc,
                    EstimatedDeliveryAtUtc = s.EstimatedDeliveryAtUtc,
                    CreatedAtUtc = s.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return new Response { Items = shipments };
        }
    }

    // EXCEPTION: shipment list DTO — no shared shipment response model exists yet
    public sealed record Response
    {
        public IReadOnlyList<ShipmentListItem> Items { get; init; } = [];
    }

    // EXCEPTION: shipment list item DTO — no shared shipment response model exists yet
    public sealed record ShipmentListItem
    {
        public Guid Id { get; init; }
        public Guid OrderId { get; init; }
        public Guid ShippingMethodId { get; init; }
        public string? TrackingNumber { get; init; }
        public ShipmentStatus Status { get; init; }
        public DateTimeOffset? ShippedAtUtc { get; init; }
        public DateTimeOffset? DeliveredAtUtc { get; init; }
        public DateTimeOffset? EstimatedDeliveryAtUtc { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ShippingFeature.Admin.Shipments.ListForOrder.Route, async (
                [FromQuery] Guid orderId,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(orderId), ct);
                return result.ToResult();
            })
            .WithName(nameof(ListShipmentsForOrder))
            .WithTags(ShippingFeature.Tags.Shipment)
            .HasPermission(ShippingFeature.Admin.Shipments.ListForOrder.Permission)
            .WithSummary(ShippingFeature.Admin.Shipments.ListForOrder.Summary)
            .WithDescription(ShippingFeature.Admin.Shipments.ListForOrder.Description)
            .Produces<Result<Response>>();
        }
    }
}
