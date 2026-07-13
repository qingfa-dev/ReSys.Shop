namespace Module.Ordering.Features.Storefront.Cart.UpdateCheckout;

public static partial class UpdateCheckout
{
    /// <summary>Validates the update-checkout command: request body must be provided.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Request body must not be null.
            RuleFor(x => x.Request)
                .NotNull();
        }
    }
}