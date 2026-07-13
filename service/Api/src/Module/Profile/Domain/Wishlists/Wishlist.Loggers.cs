namespace Module.Profile.Domain.Wishlists;

public static partial class WishlistLoggers
{
    public static partial class Management
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Debug,
            Message = "[Wishlist.Created]: {Name} ({Id}) by {ActionBy}")]
        public static partial void Created(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Debug,
            Message = "[Wishlist.Updated]: {Name} ({Id}) by {ActionBy}")]
        public static partial void Updated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Debug,
            Message = "[Wishlist.Deleted]: {Name} ({Id}) by {ActionBy}")]
        public static partial void Deleted(ILogger logger, string Name, Guid Id, string? ActionBy = "System");
    }
}