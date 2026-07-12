using Module.Ordering.Features.Storefront.Cart.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Validators;

/// <summary>Shared cart parameter validation rules for storefront features.</summary>
public static partial class CartValidator
{
    /// <summary>Validates individual cart parameters: variant ID and quantity.</summary>
    public sealed class CartParametersValidator : AbstractValidator<CartParameters>
    {
        public CartParametersValidator()
        {
            // Validate: Variant ID must not be empty.
            RuleFor(x => x.VariantId).NotEmpty();
            // Validate: Quantity must be greater than zero.
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }

    /// <summary>Applies cart parameter validation rules to a rule builder.</summary>
    public static IRuleBuilderOptions<T, CartParameters> ApplyCartParametersRules<T>(
       this IRuleBuilder<T, CartParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new CartParametersValidator());
    }
}
