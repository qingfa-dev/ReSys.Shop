using FluentValidation;

namespace Module.Ordering.Features.Storefront.Cart.ChangeCurrency;

public static partial class ChangeCartCurrency
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");

            RuleFor(x => x.Request.Currency)
                .NotEmpty()
                .Length(3)
                .WithErrorCode("Currency.Required")
                .WithMessage("A valid 3-letter ISO currency code is required.");
        }
    }
}
