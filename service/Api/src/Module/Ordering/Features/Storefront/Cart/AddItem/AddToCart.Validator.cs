namespace Module.Ordering.Features.Storefront.Cart.AddItem;

public static partial class AddToCart
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).NotNull();
            RuleFor(x => x.Request.VariantId).NotEmpty();
            RuleFor(x => x.Request.Quantity).GreaterThan(0);
        }
    }
}
