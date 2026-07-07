namespace Module.Shipping.Features.Storefront.Shipping.Calculate;

public static partial class CalculateShipping
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.OrderId).NotEmpty().WithMessage("Order ID is required.");
            RuleFor(x => x.Request.ShippingMethodId).NotEmpty().WithMessage("Shipping method ID is required.");
        }
    }
}
