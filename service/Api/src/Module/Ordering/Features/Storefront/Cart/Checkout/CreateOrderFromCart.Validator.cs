namespace Module.Ordering.Features.Storefront.Cart.Checkout;

public static partial class CreateOrderFromCart
{
    /// <summary>Validates the create-order command: request body must be provided.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Request body must not be null.
            RuleFor(x => x.Request).NotNull();
        }
    }
}