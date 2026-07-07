using Microsoft.Extensions.Logging;

namespace Module.Ordering.Domain.LineItems;

// Log: Structured logging events for LineItem lifecycle operations
public static partial class LineItemLoggers
{
    #region Create
    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Debug,
        Message = "[LineItem.Created]: {Id} (Order: {OrderId}, Variant: {VariantId}) by {ActionBy}")]
    public static partial void Created(ILogger logger, Guid Id, Guid OrderId, Guid VariantId, string? ActionBy = "System");
    #endregion

    #region Update
    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Debug,
        Message = "[LineItem.QuantityUpdated]: {Id} (Order: {OrderId}, Quantity: {Quantity}) by {ActionBy}")]
    public static partial void QuantityUpdated(ILogger logger, Guid Id, Guid OrderId, int Quantity, string? ActionBy = "System");
    #endregion
}
