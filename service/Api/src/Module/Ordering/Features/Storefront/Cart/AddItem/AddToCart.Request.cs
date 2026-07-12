namespace Module.Ordering.Features.Storefront.Cart.AddItem;

public static partial class AddToCart
{
    public class Request
    {
        /// <summary>Variant (SKU) identifier to add.</summary>
        public Guid VariantId { get; init; }
        /// <summary>Quantity to add (defaults to 1).</summary>
        public int Quantity { get; init; } = 1;
    }
}
