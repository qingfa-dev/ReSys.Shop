using Module.Ordering.Features.Storefront.Cart.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Validators;

public static partial class CartValidator
{
    public sealed class CartParametersValidator : AbstractValidator<CartParameters>
    {
        public CartParametersValidator()
        {
            RuleFor(x => x.VariantId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }

    public static IRuleBuilderOptions<T, CartParameters> ApplyCartParametersRules<T>(
       this IRuleBuilder<T, CartParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new CartParametersValidator());
    }
}
