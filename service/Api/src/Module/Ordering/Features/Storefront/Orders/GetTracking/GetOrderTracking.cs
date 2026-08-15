using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Orders.GetTracking.Shared.Models;
using Module.Shipping.Domain.Shipments;

namespace Module.Ordering.Features.Storefront.Orders.GetTracking;

/// <summary>Retrieves the shipment tracking timeline for a customer order.</summary>
public static partial class GetOrderTracking
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Finds the order by ID scoped to the current user and returns all tracking timestamps.</summary>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.NotFound(query.Id);

            var entity = await dbContext.Set<Order>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.Id && x.UserId == userId, cancellationToken);

            if (entity is null)
                return OrderResult.Errors.NotFound(query.Id);

            // Shipment: Tracking delivery timestamps live on the order's shipment.
            var shipment = await dbContext.Set<Shipment>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.OrderId == entity.Id, cancellationToken);

            return new Response
            {
                OrderId = entity.Id,
                OrderCreatedAt = entity.CreatedAtUtc,
                OrderApprovedAt = entity.ApprovedAtUtc,
                OrderCompletedAt = entity.CompletedAtUtc,
                OrderCanceledAt = entity.CanceledAtUtc,
                PaymentProcessingAt = entity.PaymentProcessingAtUtc,
                PaymentCompletedAt = entity.PaymentCompletedAtUtc,
                PaymentFailedAt = entity.PaymentFailedAtUtc,
                ShippedAt = entity.ShipmentShippedAtUtc,
                DeliveredAt = entity.ShipmentDeliveredAtUtc,
                DeliveryExceptionAt = null,
                EstimatedDeliveryAt = shipment?.EstimatedDeliveryAtUtc,
            };
        }
    }
}
