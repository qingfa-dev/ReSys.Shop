using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.RemoveItem;

public static partial class RemoveCartItem
{
    /// <summary>Validates the remove-cart-item command — line item ID must be provided.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Line item ID is required to identify the cart item to remove.
            RuleFor(x => x.LineItemId)
                .NotEmpty()
                .WithErrorCode(OrderResult.Errors.CartLineItemIdRequired.Code)
                .WithMessage(OrderResult.Errors.CartLineItemIdRequired.Message);
        }
    }
}