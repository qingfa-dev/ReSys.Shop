namespace Module.Inventory.Domain.StockTransfers;

public static class StockTransferExtensions
{
    #region Factory Methods
    /// <summary>
    /// Creates a new stock transfer in Draft state.
    /// </summary>
    /// <param name="reference">Optional external reference.</param>
    /// <param name="sourceLocationId">The source stock location identifier.</param>
    /// <param name="destinationLocationId">The destination stock location identifier.</param>
    /// <param name="items">The variants and quantities to transfer.</param>
    /// <returns>A result containing the created stock transfer.</returns>
    // Contract: pre=sourceLocationId!=Guid.Empty && destinationLocationId!=Guid.Empty && items is not empty, post=transfer.Id!=Guid.Empty
    public static Result<StockTransfer> Create(
        string? reference,
        Guid sourceLocationId,
        Guid destinationLocationId,
        List<(Guid VariantId, int Quantity)> items)
    {
        // Validate: Source and destination must differ
        if (sourceLocationId == destinationLocationId)
            return StockTransferResult.Failure.SameLocation;

        // Validate: Must have at least one item
        if (items is null || items.Count == 0)
            return StockTransferResult.Failure.NoItems;

        // Validate: All quantities must be positive
        if (items.Any(i => i.Quantity <= 0))
            return StockTransferResult.Failure.InvalidQuantity;

        var transfer = new StockTransfer
        {
            Id = Guid.NewGuid(),
            Number = GenerateNumber(),
            Reference = reference,
            SourceLocationId = sourceLocationId,
            DestinationLocationId = destinationLocationId,
            State = TransferState.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        foreach (var (variantId, quantity) in items)
        {
            transfer.TransferItems.Add(new TransferItem
            {
                Id = Guid.NewGuid(),
                StockTransferId = transfer.Id,
                VariantId = variantId,
                Quantity = quantity,
                ReceivedQuantity = 0
            });
        }

        return Result<StockTransfer>.Ok(transfer, StockTransferResult.Success.Created(transfer.Id));
    }

    private static string GenerateNumber()
    {
        return $"T{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
    }
    #endregion

    #region State Transitions
    /// <summary>
    /// Transitions the transfer from Draft to InTransit.
    /// Stock is decremented from the source location at this point.
    /// </summary>
    // Contract: pre=state==Draft, post=state==InTransit
    public static Result Transfer(this StockTransfer transfer)
    {
        if (transfer.State != TransferState.Draft)
            return StockTransferResult.Failure.InvalidStateTransition(transfer.State, TransferState.InTransit);

        transfer.State = TransferState.InTransit;
        transfer.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(StockTransferResult.Success.Transferred(transfer.Id));
    }

    /// <summary>
    /// Records receipt of items at the destination.
    /// Transitions to Received when all items are fully received.
    /// </summary>
    /// <param name="transfer">The stock transfer.</param>
    /// <param name="variantId">The variant being received.</param>
    /// <param name="quantity">The quantity received.</param>
    // Contract: pre=state==InTransit, post=ReceivedQuantity increased; auto-transitions to Received when fully received
    public static Result Receive(this StockTransfer transfer, Guid variantId, int quantity)
    {
        if (transfer.State != TransferState.InTransit)
            return StockTransferResult.Failure.InvalidStateTransition(transfer.State, TransferState.Received);

        var item = transfer.TransferItems.FirstOrDefault(i => i.VariantId == variantId);
        if (item is null)
            return StockTransferResult.Failure.VariantNotInTransfer(variantId);

        var newReceived = item.ReceivedQuantity + quantity;
        if (newReceived > item.Quantity)
            return StockTransferResult.Failure.ReceivedExceedsTransferred(variantId, item.Quantity, newReceived);

        item.ReceivedQuantity = newReceived;
        transfer.ModifiedAtUtc = DateTimeOffset.UtcNow;

        // Auto-transition to Received when all items fully received
        if (transfer.TransferItems.All(i => i.ReceivedQuantity >= i.Quantity))
        {
            transfer.State = TransferState.Received;
        }

        return Result.Ok(StockTransferResult.Success.Received(transfer.Id));
    }

    /// <summary>
    /// Cancels the transfer. Stock is restored to source if already InTransit.
    /// </summary>
    // Contract: pre=state==Draft || state==InTransit, post=state==Canceled
    public static Result Cancel(this StockTransfer transfer)
    {
        if (transfer.State != TransferState.Draft && transfer.State != TransferState.InTransit)
            return StockTransferResult.Failure.InvalidStateTransition(transfer.State, TransferState.Canceled);

        transfer.State = TransferState.Canceled;
        transfer.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(StockTransferResult.Success.Canceled(transfer.Id));
    }
    #endregion
}
