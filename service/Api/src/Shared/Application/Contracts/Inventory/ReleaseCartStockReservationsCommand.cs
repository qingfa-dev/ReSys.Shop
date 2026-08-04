using Shared.Application.Mediators.Commands;

namespace Shared.Application.Contracts.Inventory;

public sealed record ReleaseCartStockReservationsCommand : ICommand
{
    public Guid CartId { get; init; }
}
