using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Shared.Models;

/// <summary>Full order detail response DTO — used for single-order queries and mutation responses.</summary>
public class OrderDetailResponse : OrderParameters
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    /// <summary>Current lifecycle status of the order (Draft, Placed, Canceled, etc.).</summary>
    public OrderStatus Status { get; init; }
    /// <summary>Checkout workflow stage — tracks progress through the multi-step checkout.</summary>
    public CheckoutState CheckoutState { get; init; }
    public decimal ItemTotal { get; init; }
    /// <summary>Sum of all adjustments applied to line items (discounts, surcharges).</summary>
    public decimal AdjustmentTotal { get; init; }
    public decimal ShipmentTotal { get; init; }
    /// <summary>Grand total — ItemTotal + AdjustmentTotal + ShipmentTotal.</summary>
    public decimal Total { get; init; }
    public decimal PaymentTotal { get; init; }
    /// <summary>Remaining balance after payments applied — zero when fully paid.</summary>
    public decimal OutstandingBalance { get; init; }
    /// <summary>Payment processing status — tracks gateway interaction state.</summary>
    public string? PaymentState { get; init; }
    /// <summary>Shipment fulfillment status — tracks carrier and delivery progress.</summary>
    public string? ShipmentState { get; init; }
    public Guid? UserId { get; init; }
    public Guid? StoreId { get; init; }
    public int ItemCount { get; init; }
    public Guid? ApprovedById { get; init; }
    public DateTimeOffset? ApprovedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
    public DateTimeOffset? CanceledAtUtc { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
}

/// <summary>Summary order response DTO — used for paginated admin order grid lists.</summary>
public class OrderListItemResponse : OrderParameters
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    /// <summary>Current lifecycle status — enables row-level styling in the admin grid.</summary>
    public OrderStatus Status { get; init; }
    public decimal Total { get; init; }
    public decimal PaymentTotal { get; init; }
    /// <summary>Payment processing status — tracks gateway interaction state.</summary>
    public string? PaymentState { get; init; }
    /// <summary>Shipment fulfillment status — tracks carrier and delivery progress.</summary>
    public string? ShipmentState { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
}
