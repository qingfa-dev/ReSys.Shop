using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;

public static partial class UpdateCartItemQuantity
{
    /// <summary>Validates the update-cart-item-quantity command — line item ID, request body, and quantity range.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Line item ID is required to identify which cart item to update.
            RuleFor(x => x.LineItemId)
                .NotEmpty()
                .WithErrorCode(OrderResult.Errors.CartLineItemIdRequired.Code)
                .WithMessage(OrderResult.Errors.CartLineItemIdRequired.Message);

            // Validate: Request body must be provided for quantity update.
            RuleFor(x => x.Request)
                .NotNull()
                .WithErrorCode(OrderResult.Errors.CartRequestRequired.Code)
                .WithMessage(OrderResult.Errors.CartRequestRequired.Message);

            When(x => x.Request is not null, () =>
            {
                // Validate: Quantity must be within the allowed range.
                RuleFor(x => x.Request.Quantity)
                    .ApplyQuantityRules();
            });
        }
    }
}