using Shared.Application.Mediators.Commands;

namespace Shared.Application.Contracts.Inventory;

public sealed record ConsumeCartStockReservationsCommand : ICommand<ConsumeCartStockReservationsResponse>
{
    public Guid CartId { get; init; }
}

public sealed record ConsumeCartStockReservationsResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
