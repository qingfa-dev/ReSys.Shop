using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;

public static partial class UpdateCartItemQuantity
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.LineItemId)
                .NotEmpty()
                .WithErrorCode("Cart.LineItemId.Required")
                .WithMessage("Line item ID is required.");

            RuleFor(x => x.Request)
                .NotNull()
                .WithErrorCode("Cart.Request.Required")
                .WithMessage("Request body is required.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request!.Quantity)
                    .InclusiveBetween(1, LineItemConstant.MaxQuantity)
                    .WithErrorCode("Cart.Quantity.Invalid")
                    .WithMessage($"Quantity must be between 1 and {LineItemConstant.MaxQuantity}.");
            });
        }
    }
}
