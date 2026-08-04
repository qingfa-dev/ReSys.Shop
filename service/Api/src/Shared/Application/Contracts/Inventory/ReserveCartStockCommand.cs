using Shared.Application.Mediators.Commands;

namespace Shared.Application.Contracts.Inventory;

public sealed record ReserveCartStockCommand : ICommand<ReserveCartStockResponse>
{
    public Guid CartId { get; init; }
    public IReadOnlyList<ReserveLineItem> LineItems { get; init; } = [];
    public int TtlMinutes { get; init; } = 30;
}

public sealed record ReserveLineItem
{
    public Guid VariantId { get; init; }
    public int Quantity { get; init; }
}

public sealed record ReserveCartStockResponse
{
    public IReadOnlyList<Guid> ReservationIds { get; init; } = [];
    public bool Success { get; init; }
}
