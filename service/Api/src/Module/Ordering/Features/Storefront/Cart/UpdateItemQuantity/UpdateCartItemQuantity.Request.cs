namespace Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;

public static partial class UpdateCartItemQuantity
{
    public class Request
    {
        /// <summary>New quantity for the line item.</summary>
        public int Quantity { get; init; }
    }
}
