namespace Module.Ordering.Features.Storefront.Cart.AddItem;

public static partial class AddToCart
{
    public class Request
    {
        public Guid VariantId { get; init; }
        public int Quantity { get; init; } = 1;
    }
}
