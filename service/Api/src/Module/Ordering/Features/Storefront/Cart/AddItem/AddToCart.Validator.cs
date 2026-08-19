using Module.Ordering.Features.Storefront.Shared.Validators;

namespace Module.Ordering.Features.Storefront.Cart.AddItem;

public static partial class AddToCart
{
    /// <summary>Validates the add-to-cart command — request body must be present with valid variant and quantity.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Request body must be provided.
            RuleFor(x => x.Request).NotNull();
            // Validate: Variant ID and quantity via shared cart parameters rules.
            RuleFor(x => x.Request)
                .ApplyCartParametersRules();
        }
    }
}