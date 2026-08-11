using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Shared.Models;

public record OrderDetailResponse : OrderParameters
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public CheckoutState CheckoutState { get; init; }
    public decimal ItemTotal { get; init; }
    public decimal AdjustmentTotal { get; init; }
    public decimal ShipmentTotal { get; init; }
    public decimal Total { get; init; }
    public decimal PaymentTotal { get; init; }
    public decimal OutstandingBalance { get; init; }
    public string? PaymentState { get; init; }
    public string? ShipmentState { get; init; }
    public Guid? UserId { get; init; }
    public int ItemCount { get; init; }
    public Guid? ApprovedById { get; init; }
    public DateTimeOffset? ApprovedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
    public DateTimeOffset? CanceledAtUtc { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}

public record OrderListItemResponse : OrderParameters
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public decimal Total { get; init; }
    public decimal PaymentTotal { get; init; }
    public string? PaymentState { get; init; }
    public string? ShipmentState { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
}

public record LineItemResponse
{
    public Guid Id { get; init; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public decimal AdjustmentTotal { get; set; }
    public string Currency { get; set; } = OrderConstant.Defaults.Currency;

    public Guid OrderId { get; set; }
    public Guid? VariantId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}