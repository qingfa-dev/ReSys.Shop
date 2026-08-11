namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region Factory

    // Create: New draft order with default pending status, zero totals, and UTC audit timestamp
    public static Result<Order> Create(
        string currency,
        Guid? userId,
        Guid? id = null,
        string? sessionId = null,
        Guid? shipAddressId = null)
    {
        // Assign: Generate a unique temporary identifier for the draft — replaced with permanent number at Finalize
        var order = new Order
        {
            Id = id ?? Guid.NewGuid(),
            Number = $"DRAFT-{Guid.NewGuid():N}",
            SessionId = sessionId,
            Status = OrderStatus.Draft,
            CheckoutState = CheckoutState.Address,
            Currency = currency,
            UserId = userId,
            ShipAddressId = shipAddressId,
            ItemTotal = 0m,
            AdjustmentTotal = 0m,
            ShipmentTotal = 0m,
            Total = 0m,
            PaymentTotal = 0m,
            OutstandingBalance = 0m,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = OrderConstant.Defaults.CreatedBy
        };

        return order;
    }

    #endregion
}