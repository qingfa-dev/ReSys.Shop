namespace Module.Ordering.Features.Storefront.Cart.SelectShippingRate;

public static partial class SelectShippingRate
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .NotNull();

            RuleFor(x => x.Request.ShippingMethodId)
                .NotEmpty()
                .WithErrorCode("ShippingRate.Selection.MethodRequired")
                .WithMessage("Shipping method is required.");
        }
    }
}
