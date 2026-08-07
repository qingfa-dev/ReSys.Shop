namespace Module.Inventory.Features.Storefront.ReleaseCartStockReservations;

public sealed record ReleaseCartStockReservationsCommand : ICommand
{
    public Guid CartId { get; init; }
}
