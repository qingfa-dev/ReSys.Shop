namespace Module.Inventory.Services.Abstractions;

public interface IStockQuantityService
{
    Task<Result> DecrementStockAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<Result> IncrementStockAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        Guid orderId,
        CancellationToken cancellationToken = default);
}
