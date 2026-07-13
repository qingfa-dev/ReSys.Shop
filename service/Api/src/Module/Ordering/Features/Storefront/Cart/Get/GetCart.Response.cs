using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.Get;

public static partial class GetCart
{
    public class Response
    {
        public Guid? Id { get; init; }
        public List<CartItem> Items { get; init; } = [];
        public decimal ItemTotal { get; init; }
        public decimal Total { get; init; }
        public string Currency { get; init; } = OrderConstant.Defaults.Currency;
        public int ItemCount { get; init; }
        public string CheckoutState { get; init; } = string.Empty;
    }

    public class CartItem
    {
        public Guid Id { get; init; }
        public Guid VariantId { get; init; }
        public string VariantName { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal Price { get; init; }
        public decimal Total { get; init; }
    }
}
