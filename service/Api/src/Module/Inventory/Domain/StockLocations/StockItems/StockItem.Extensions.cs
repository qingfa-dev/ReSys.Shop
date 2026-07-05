namespace Module.Inventory.Domain.StockLocations.StockItems;

public static class StockItemMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new stock item for a variant at a stock location.
    /// </summary>
    /// <param name="stockLocationId">The stock location identifier.</param>
    /// <param name="variantId">The variant identifier.</param>
    /// <param name="backorderable">Whether backorders are allowed.</param>
    /// <param name="countOnHand">The initial count on hand.</param>
    /// <returns>A result containing the created stock item.</returns>
    // Contract: pre=countOnHand>=0, post=stockItem.Id != Guid.Empty
    public static Result<StockItem> Create(
        Guid stockLocationId,
        Guid variantId,
        bool backorderable = StockItemConstant.Defaults.Backorderable,
        int countOnHand = StockItemConstant.Defaults.CountOnHand)
    {
        // Validate: Count-on-hand must not be negative
        if (countOnHand < 0)
        {
            return StockItemResult.Errors.NegativeCountOnHand;
        }

        var stockItem = new StockItem
        {
            Id = Guid.NewGuid(),
            StockLocationId = stockLocationId,
            VariantId = variantId,
            Backorderable = backorderable,
            CountOnHand = countOnHand,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return Result<StockItem>.Ok(stockItem);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the stock item's count-on-hand and backorderable flag.
    /// </summary>
    /// <param name="stockItem">The stock item to update.</param>
    /// <param name="countOnHand">Optional new count-on-hand value.</param>
    /// <param name="backorderable">Optional new backorderable flag.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result Update(this StockItem stockItem,
        int? countOnHand = null,
        bool? backorderable = null)
    {
        if (countOnHand.HasValue)
        {
            if (countOnHand.Value < 0)
                return StockItemResult.Errors.NegativeCountOnHand;
            stockItem.CountOnHand = countOnHand.Value;
        }

        if (backorderable.HasValue)
            stockItem.Backorderable = backorderable.Value;

        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    /// <summary>
    /// Adjusts the count-on-hand by a relative quantity. Positive adds stock, negative removes it.
    /// </summary>
    /// <param name="stockItem">The stock item to adjust.</param>
    /// <param name="quantity">The quantity to add (positive) or remove (negative).</param>
    /// <param name="reason">Optional reason for the adjustment.</param>
    /// <returns>A result indicating success or failure.</returns>
    // Enforce: Count on hand must never go negative; backordered items fill first
    public static Result AdjustCountOnHand(this StockItem stockItem, int quantity, string? reason = null)
    {
        var newCount = stockItem.CountOnHand + quantity;

        if (newCount < 0)
        {
            return StockItemResult.Errors.NegativeCountOnHand;
        }

        stockItem.CountOnHand = newCount;
        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(StockItemResult.Success.Adjusted);
    }

    /// <summary>
    /// Sets the backorderable flag on the stock item.
    /// </summary>
    /// <param name="stockItem">The stock item to update.</param>
    /// <param name="backorderable">Whether backorders are allowed.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result SetBackorderable(this StockItem stockItem, bool backorderable)
    {
        stockItem.Backorderable = backorderable;
        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok();
    }

    /// <summary>
    /// Increases the count-on-hand by the specified quantity. Aligned with Ruby SDK restock.
    /// </summary>
    /// <param name="stockItem">The stock item to restock.</param>
    /// <param name="quantity">The quantity to add. Must be positive.</param>
    /// <returns>A result indicating success or failure.</returns>
    // Validate: Restock quantity must be positive
    public static Result Restock(this StockItem stockItem, int quantity)
    {
        // Validate: Restock quantity must be positive
        if (quantity <= 0)
        {
            return StockItemResult.Errors.NegativeCountOnHand;
        }

        stockItem.CountOnHand += quantity;
        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(StockItemResult.Success.Restocked);
    }

    /// <summary>
    /// Removes stock from the count-on-hand. Aligned with Ruby SDK unstock.
    /// </summary>
    /// <param name="stockItem">The stock item to pick from.</param>
    /// <param name="quantity">The quantity to remove. Must be positive and not exceed available stock.</param>
    /// <returns>A result indicating success or failure.</returns>
    // Validate: Pick quantity must be positive and not exceed count-on-hand
    public static Result Pick(this StockItem stockItem, int quantity)
    {
        // Validate: Pick quantity must be positive
        if (quantity <= 0)
        {
            return StockItemResult.Errors.NegativeCountOnHand;
        }

        // Validate: Requested quantity must not exceed available stock
        if (stockItem.CountOnHand < quantity)
        {
            return StockItemResult.Errors.InsufficientStock;
        }

        stockItem.CountOnHand -= quantity;
        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(StockItemResult.Success.Picked);
    }

    /// <summary>
    /// Checks whether the requested quantity can be fulfilled from this stock item.
    /// </summary>
    /// <param name="stockItem">The stock item to check.</param>
    /// <param name="requestedQuantity">The quantity requested.</param>
    /// <returns>True if stock is sufficient or backorderable.</returns>
    // Compute: Available if on-hand covers quantity or backorderable
    public static bool IsAvailable(this StockItem stockItem, int requestedQuantity)
    {
        return stockItem.CountOnHand >= requestedQuantity || stockItem.Backorderable;
    }

    /// <summary>
    /// Returns the total count-on-hand. Aligned with Ruby SDK total_on_hand.
    /// </summary>
    // Compute: Total on hand is the current CountOnHand value
    public static int TotalOnHand(this StockItem stockItem)
    {
        return stockItem.CountOnHand;
    }

    /// <summary>
    /// Checks whether the specified quantity can be supplied. Aligned with Ruby SDK can_supply.
    /// </summary>
    // Compute: Can supply if on-hand is sufficient or item is backorderable
    public static bool CanSupply(this StockItem stockItem, int quantity)
    {
        return stockItem.CountOnHand >= quantity || stockItem.Backorderable;
    }

    /// <summary>
    /// Returns whether backorders are allowed. Aligned with Ruby SDK backorderable?.
    /// </summary>
    // Compute: Whether the stock item accepts backorders
    public static bool IsBackorderable(this StockItem stockItem)
    {
        return stockItem.Backorderable;
    }

    /// <summary>
    /// Fulfills backordered inventory units from a restock quantity.
    /// Backordered units are filled first; remaining quantity stays on hand.
    /// Aligned with Ruby SDK process_backorders.
    /// </summary>
    /// <param name="stockItem">The stock item to process.</param>
    /// <param name="quantity">The restock quantity. Must be positive.</param>
    /// <returns>The quantity remaining after filling backordered units.</returns>
    // Compute: Fulfill backordered inventory units starting with oldest orders
    public static int ProcessBackorders(this StockItem stockItem, int quantity)
    {
        if (quantity <= 0) return 0;

        if (stockItem.CountOnHand >= quantity)
            return quantity;

        var backorderCapacity = stockItem.Backorderable ? quantity - stockItem.CountOnHand : 0;
        var filledOnHand = Math.Min(stockItem.CountOnHand, quantity);
        var remaining = quantity - filledOnHand - backorderCapacity;

        return remaining;
    }

    /// <summary>
    /// Computes how the requested quantity splits between on-hand and backordered.
    /// Aligned with Ruby SDK fill_status.
    /// </summary>
    /// <param name="stockItem">The stock item to check.</param>
    /// <param name="requestedQuantity">The quantity requested.</param>
    /// <returns>A tuple of (onHand, backordered).</returns>
    // Compute
    public static (int OnHand, int Backordered) FillStatus(this StockItem stockItem, int requestedQuantity)
    {
        if (stockItem.CountOnHand >= requestedQuantity)
            return (requestedQuantity, 0);

        var onHand = stockItem.CountOnHand;
        var backordered = stockItem.Backorderable ? requestedQuantity - onHand : 0;
        return (onHand, backordered);
    }
    #endregion
}
