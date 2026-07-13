using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Models;

public abstract record CartResponseBase
{
    public Guid Id { get; init; }
    public decimal ItemTotal { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = OrderConstant.Defaults.Currency;
    public int ItemCount { get; init; }
    public string CheckoutState { get; init; } = string.Empty;
}
