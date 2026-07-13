namespace Module.Ordering.Features.Storefront.Cart.AddItem;

public static partial class AddToCart
{
    public sealed record Response
    {
        /// <summary>Identifier of the newly created line item.</summary>
        public Guid LineItemId { get; init; }
    }
}
