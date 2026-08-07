namespace Module.Inventory.Features.Storefront.CartReservations.ReleaseSingle;

public sealed record ReleaseSingleReservationCommand : ICommand
{
    public Guid ReservationId { get; init; }
}
