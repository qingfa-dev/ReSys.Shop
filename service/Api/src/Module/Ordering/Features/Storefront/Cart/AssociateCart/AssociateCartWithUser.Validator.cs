namespace Module.Ordering.Features.Storefront.Cart.AssociateCart;

public static partial class AssociateCartWithUser
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.GuestOrderId)
                .NotEmpty()
                .WithErrorCode("Cart.GuestOrderId.Required")
                .WithMessage("Guest order ID is required.");
        }
    }
}
