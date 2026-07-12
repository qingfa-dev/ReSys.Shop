namespace Module.Ordering.Features.Storefront.Cart.AssociateCart;

public static partial class AssociateCartWithUser
{
    /// <summary>Validates the associate-cart command: guest order ID must be provided.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Guest order ID must not be empty.
            RuleFor(x => x.Request.GuestOrderId)
                .NotEmpty()
                .WithErrorCode("Cart.GuestOrderId.Required")
                .WithMessage("Guest order ID is required.");
        }
    }
}
