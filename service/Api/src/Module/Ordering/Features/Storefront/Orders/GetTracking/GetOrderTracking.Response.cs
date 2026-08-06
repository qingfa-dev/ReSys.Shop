namespace Module.Ordering.Features.Storefront.Orders.GetTracking;

public static partial class GetOrderTracking
{
    public sealed record Response
    {
        public Guid OrderId { get; init; }
        public DateTimeOffset OrderCreatedAt { get; init; }
        public DateTimeOffset? OrderApprovedAt { get; init; }
        public DateTimeOffset? OrderCompletedAt { get; init; }
        public DateTimeOffset? OrderCanceledAt { get; init; }
        public DateTimeOffset? PaymentProcessingAt { get; init; }
        public DateTimeOffset? PaymentCompletedAt { get; init; }
        public DateTimeOffset? PaymentFailedAt { get; init; }
        public DateTimeOffset? ShippedAt { get; init; }
        public DateTimeOffset? DeliveredAt { get; init; }
        public DateTimeOffset? DeliveryExceptionAt { get; init; }
        public DateTimeOffset? EstimatedDeliveryAt { get; init; }
    }
}
