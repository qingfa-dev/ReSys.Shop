namespace Module.Ordering.Features.Storefront.Cart.RemoveItem;

public static partial class RemoveCartItem
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.LineItemId)
                .NotEmpty()
                .WithErrorCode("Cart.LineItemId.Required")
                .WithMessage("Line item ID is required.");
        }
    }
}
