using Shared.Application.Mediators.Queries;

namespace Shared.Application.Contracts.Ordering;

public sealed record GetCartForShippingQuery(Guid CartId) : IQuery<CartForShippingResponse>;

public sealed record CartForShippingResponse
{
    public decimal TotalWeight { get; init; }
    public decimal TotalValue { get; init; }
    public Guid? ShipAddressId { get; init; }
    public string Currency { get; init; } = default!;
}
