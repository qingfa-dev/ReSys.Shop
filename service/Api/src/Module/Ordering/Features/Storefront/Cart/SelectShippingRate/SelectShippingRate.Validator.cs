using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.SelectShippingRate;

public static partial class SelectShippingRate
{
    /// <summary>Validates the select-shipping-rate command: request body and shipping method ID.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Request body must be provided.
            RuleFor(x => x.Request)
                .NotNull();

            // Validate: Shipping method ID must not be empty.
            RuleFor(x => x.Request.ShippingMethodId)
                .ApplyShippingMethodIdRules();
        }
    }
}