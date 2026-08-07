namespace Module.Inventory.Features.Storefront.ConsumeCartStockReservations;

public sealed record ConsumeCartStockReservationsCommand : ICommand
{
    public Guid CartId { get; init; }
}
