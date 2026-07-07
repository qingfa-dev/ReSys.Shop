using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Domain.Shipments;

public static class ShipmentExtensions
{
    #region Factory Methods
    /// <summary>
    /// Creates a new shipment for the specified order and stock location.
    /// </summary>
    /// <param name="orderId">The order identifier. Must not be empty.</param>
    /// <param name="stockLocationId">The stock location identifier. Must not be empty.</param>
    /// <param name="id">Optional explicit identifier. A new GUID is generated if omitted.</param>
    /// <returns>A result containing the newly created shipment in Pending state.</returns>
    // @CAT-10 Contract: pre=orderId!=default && stockLocationId!=default, post=shipment.Id!=default && shipment.State==Pending, throws=none
    public static Result<Shipment> Create(
        Guid orderId,
        Guid stockLocationId,
        Guid? id = null)
    {
        var shipment = new Shipment
        {
            Id = id ?? Guid.NewGuid(),
            Number = string.Empty,
            State = ShipmentConstant.Defaults.State,
            OrderId = orderId,
            StockLocationId = stockLocationId,
            Cost = 0m,
            DiscountedCost = 0m,
            FinalPrice = 0m,
            ItemCost = 0m,
            AdditionalTaxTotal = 0m,
            IncludedTaxTotal = 0m,
            TaxTotal = 0m,
            PromoTotal = 0m,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return Result.Ok(shipment);
    }
    #endregion Factory Methods

    #region State Machine
    /// <summary>
    /// Transitions the shipment from Pending to Ready state.
    /// </summary>
    /// <param name="shipment">The shipment to transition.</param>
    /// <returns>A result indicating success or failure with an error message.</returns>
    // @CAT-4 Enforce: Shipment must be in Pending state to transition to Ready
    public static Result Ready(this Shipment shipment)
    {
        if (!ShipmentValidation.IsValidTransition(shipment.State, ShipmentState.Ready))
        {
            return ShipmentResult.Errors.InvalidStateTransition(shipment.State, ShipmentState.Ready);
        }

        shipment.State = ShipmentState.Ready;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(ShipmentResult.Success.Ready(shipment.Id));
    }

    /// <summary>
    /// Ships the shipment with the provided tracking number.
    /// </summary>
    /// <param name="shipment">The shipment to ship.</param>
    /// <param name="tracking">The tracking number. Must not be null or whitespace.</param>
    /// <returns>A result indicating success or failure.</returns>
    // @CAT-4 Enforce: Shipment must be in Ready state to transition to Shipped
    // Guard: Prevent shipping without a tracking number
    public static Result Ship(this Shipment shipment, string tracking)
    {
        if (string.IsNullOrWhiteSpace(tracking))
        {
            return ShipmentResult.Errors.TrackingRequired;
        }

        if (!ShipmentValidation.IsValidTransition(shipment.State, ShipmentState.Shipped))
        {
            return ShipmentResult.Errors.InvalidStateTransition(shipment.State, ShipmentState.Shipped);
        }

        shipment.State = ShipmentState.Shipped;
        shipment.Tracking = tracking;
        shipment.ShippedAtUtc = DateTimeOffset.UtcNow;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(ShipmentResult.Success.Shipped(shipment.Id));
    }

    /// <summary>
    /// Cancels the shipment. Allowed from Pending or Ready states only.
    /// </summary>
    /// <param name="shipment">The shipment to cancel.</param>
    /// <returns>A result indicating success or failure.</returns>
    // @CAT-4 Enforce: Shipment must be in Pending or Ready state to be canceled
    public static Result Cancel(this Shipment shipment)
    {
        if (!ShipmentValidation.IsValidTransition(shipment.State, ShipmentState.Canceled))
        {
            return ShipmentResult.Errors.InvalidStateTransition(shipment.State, ShipmentState.Canceled);
        }

        shipment.State = ShipmentState.Canceled;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(ShipmentResult.Success.Canceled(shipment.Id));
    }

    /// <summary>
    /// Refreshes shipping rates by clearing existing rates for re-query.
    /// </summary>
    /// <param name="shipment">The shipment to refresh rates for.</param>
    /// <returns>A result indicating success or failure.</returns>
    // @CAT-5 Compute: Clear existing rates for re-query from rate calculator (Ruby SDK refresh_rates alignment)
    public static Result RefreshRates(this Shipment shipment)
    {
        if (shipment.State == ShipmentState.Shipped)
        {
            return ShipmentResult.Errors.AlreadyShipped;
        }

        if (shipment.State == ShipmentState.Canceled)
        {
            return ShipmentResult.Errors.AlreadyCanceled;
        }

        shipment.ShippingRates.Clear();
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok();
    }

    /// <summary>
    /// Selects a shipping rate for the shipment and updates cost and final price.
    /// </summary>
    /// <param name="shipment">The shipment to select a rate for.</param>
    /// <param name="shippingRateId">The identifier of the rate to select.</param>
    /// <returns>A result indicating success or failure.</returns>
    // @CAT-4 Enforce: Only one rate selected per shipment — unselects all others first
    public static Result SelectShippingRate(this Shipment shipment, Guid shippingRateId)
    {
        if (shipment.ShippingRates.Count == 0)
        {
            return ShipmentResult.Errors.NoShippingRates;
        }

        var rate = shipment.ShippingRates.FirstOrDefault(r => r.Id == shippingRateId);
        if (rate is null)
        {
            return ShipmentResult.Errors.NotFound(shippingRateId);
        }

        foreach (var sr in shipment.ShippingRates)
        {
            sr.Unselect();
        }

        rate.Select();
        shipment.ShippingMethodId = rate.ShippingMethodId;
        shipment.Cost = rate.Cost;
        shipment.FinalPrice = rate.FinalPrice;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(ShipmentResult.Success.RateSelected(shipment.Id));
    }

    /// <summary>
    /// Transfers the shipment to a different stock location.
    /// </summary>
    /// <param name="shipment">The shipment to transfer.</param>
    /// <param name="newStockLocationId">The target stock location identifier.</param>
    /// <returns>A result indicating success or failure.</returns>
    // Context: Manifest() groups inventory units by variant (Ruby SDK alignment)
    // Context: DetermineState() detects state from inventory and order state (Ruby SDK alignment)
    public static Result TransferToLocation(this Shipment shipment, Guid newStockLocationId)
    {
        if (shipment.State == ShipmentState.Shipped)
        {
            return ShipmentResult.Errors.AlreadyShipped;
        }

        if (shipment.State == ShipmentState.Canceled)
        {
            return ShipmentResult.Errors.AlreadyCanceled;
        }

        shipment.StockLocationId = newStockLocationId;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok();
    }
    #endregion State Machine
}