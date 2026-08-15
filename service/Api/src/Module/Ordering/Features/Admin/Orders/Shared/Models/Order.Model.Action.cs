using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Shared.Models;

/// <summary>Parameters for cancelling an order.</summary>
public abstract record OrderCancellationParameters
{
    /// <summary>Optional free-text reason for the cancellation — recorded for audit trail.</summary>
    public string? Reason { get; init; }
}

/// <summary>Parameters for updating an order's status.</summary>
public abstract record OrderStatusUpdateParameters
{
    /// <summary>The new order status.</summary>
    public OrderStatus Status { get; init; }
}

/// <summary>Parameters for associating a guest order with a signed-in user.</summary>
public abstract record CartAssociationParameters
{
    /// <summary>The guest order to associate.</summary>
    public Guid GuestOrderId { get; init; }
}

/// <summary>Parameters for creating an order from the cart.</summary>
public abstract record OrderCreationParameters
{
    /// <summary>The payment intent that settled the order.</summary>
    public string? PaymentIntentId { get; init; }
}
