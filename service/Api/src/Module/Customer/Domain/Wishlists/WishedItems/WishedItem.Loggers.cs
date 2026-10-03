namespace Module.Customer.Domain.Wishlists.WishedItems;

public static partial class WishedItemLoggers
{
    public static partial class Management
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Debug,
            Message = "[WishedItem.Created]: Variant {VariantId} added to wishlist {WishlistId}")]
        public static partial void Created(ILogger logger, Guid VariantId, Guid WishlistId);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Debug,
            Message = "[WishedItem.Updated]: Variant {VariantId} quantity changed in wishlist {WishlistId}")]
        public static partial void Updated(ILogger logger, Guid VariantId, Guid WishlistId);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Debug,
            Message = "[WishedItem.Deleted]: Variant {VariantId} removed from wishlist {WishlistId}")]
        public static partial void Deleted(ILogger logger, Guid VariantId, Guid WishlistId);
    }
}