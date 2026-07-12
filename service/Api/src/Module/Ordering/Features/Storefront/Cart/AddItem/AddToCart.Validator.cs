namespace Module.Ordering.Features.Storefront.Cart.AddItem;

public static partial class AddToCart
{
    /// <summary>Validates the add-to-cart command: request, variant ID, and quantity.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Request body must be provided.
            RuleFor(x => x.Request).NotNull();
            // Validate: Variant ID must not be empty.
            RuleFor(x => x.Request.VariantId).NotEmpty();
            // Validate: Quantity must be greater than zero.
            RuleFor(x => x.Request.Quantity).GreaterThan(0);
        }
    }
}
