using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Orders.Shared.Models;

public record StorefrontOrderListItemResponse
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public decimal Total { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}
