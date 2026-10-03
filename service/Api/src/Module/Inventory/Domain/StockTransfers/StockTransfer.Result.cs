namespace Module.Inventory.Domain.StockTransfers;

public static class StockTransferResult
{
    public static class Success
    {
        public static string Created(Guid id) => $"Stock transfer {id} created.";
        public static string Transferred(Guid id) => $"Stock transfer {id} is now in transit.";
        public static string Received(Guid id) => $"Stock transfer {id} received.";
        public static string Canceled(Guid id) => $"Stock transfer {id} canceled.";
    }
    public static class Failure
    {
        public static Error SameLocation => Error.Validation(
            code: "StockTransfer.SameLocation",
            message: "Source and destination locations must be different.");

        public static Error NoItems => Error.Validation(
            code: "StockTransfer.NoItems",
            message: "Transfer must include at least one item.");

        public static Error InvalidQuantity => Error.Validation(
            code: "StockTransfer.InvalidQuantity",
            message: "All transfer item quantities must be greater than zero.");

        public static Error NotFound => Error.NotFound(
            code: "StockTransfer.NotFound",
            message: "Stock transfer not found.");

        public static Error InsufficientStockAtSource => Error.Validation(
            code: "StockTransfer.InsufficientStockAtSource",
            message: "Source location does not have enough stock for the transfer.");

        public static Error InvalidStateTransition(TransferState current, TransferState target) =>
            Error.Validation(
                code: "StockTransfer.InvalidStateTransition",
                message: $"Cannot transition from {current} to {target}.");

        public static Error InvalidState => Error.Validation(
            code: "StockTransfer.State.Invalid",
            message: "Transfer state must be one of the defined states.");

        public static Error VariantNotInTransfer(Guid variantId) =>
            Error.NotFound(
                code: "StockTransfer.VariantNotInTransfer",
                message: $"Variant {variantId} is not part of this transfer.");

        public static Error DestinationStockItemNotFound(Guid variantId) =>
            Error.NotFound(
                code: "StockTransfer.DestinationStockItem.NotFound",
                message: $"Destination stock item for variant {variantId} was not found at the destination location.");

        public static Error ReceivedExceedsTransferred(Guid variantId, int transferred, int attempted) =>
            Error.Validation(
                code: "StockTransfer.ReceivedExceedsTransferred",
                message: $"Cannot receive {attempted} units of variant {variantId} — only {transferred} were transferred.");
    }
}