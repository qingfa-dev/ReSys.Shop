using Microsoft.EntityFrameworkCore;
using Module.Shipping.Domain.Shipments;

namespace Module.Ordering.Features.Admin.Orders.Get.ShipmentById;

/// <summary>Gets a single shipment for an order.</summary>
public static partial class GetOrderShipmentById
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string? Tracking { get; init; }
        public decimal Cost { get; init; }
        public Guid OrderId { get; init; }
        public Guid StockLocationId { get; init; }
        public Guid? ShippingMethodId { get; init; }
        public DateTimeOffset? ShippedAtUtc { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
    }

    public sealed record Query(Guid OrderId, Guid ShipmentId) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var shipment = await dbContext.Set<Shipment>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    s => s.Id == query.ShipmentId && s.OrderId == query.OrderId,
                    cancellationToken);

            if (shipment is null)
                return ShipmentResult.Errors.NotFound(query.ShipmentId);

            return new Response
            {
                Id = shipment.Id,
                Number = shipment.Number,
                State = shipment.State.ToString(),
                Tracking = shipment.Tracking,
                Cost = shipment.Cost,
                OrderId = shipment.OrderId,
                StockLocationId = shipment.StockLocationId,
                ShippingMethodId = shipment.ShippingMethodId,
                ShippedAtUtc = shipment.ShippedAtUtc,
                CreatedAtUtc = shipment.CreatedAtUtc
            };
        }
    }
}
