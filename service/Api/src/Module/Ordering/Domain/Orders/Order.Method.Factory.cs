namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region Factory

    public static Result<Order> Create(
        string currency,
        Guid? userId,
        Guid storeId,
        Guid? id = null,
        string? sessionId = null,
        Guid? shipAddressId = null)
    {
        var order = new Order
        {
            Id = id ?? Guid.NewGuid(),
            Number = $"DRAFT-{Guid.NewGuid():N}",
            SessionId = sessionId,
            Status = OrderStatus.Draft,
            CheckoutState = CheckoutState.Address,
            Currency = currency,
            UserId = userId,
            StoreId = storeId,
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
