using FluentValidation;

namespace Module.Ordering.Features.Storefront.Cart.UpdateCheckout;

public static partial class UpdateCheckout
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .NotNull();
        }
    }
}
