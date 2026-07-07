namespace Module.Ordering.Features.Storefront.Cart.Checkout;

public static partial class CreateOrderFromCart
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).NotNull();
        }
    }
}
