using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Shared.Models;

namespace Module.Ordering.Features.Storefront.Shared.Validators;

public static partial class CartValidator
{
    /// <summary>Validates cart parameters — variant ID must be provided, quantity must be positive.</summary>
    public sealed class CartParametersValidator : AbstractValidator<CartParameters>
    {
        public CartParametersValidator()
        {
            // Validate: Variant ID is required to identify the product being added.
            RuleFor(x => x.VariantId)
                .NotEmpty()
                .WithErrorCode(OrderResult.Errors.IdRequired.Code)
                .WithMessage(OrderResult.Errors.IdRequired.Message);
            // Validate: Quantity must be within allowed range.
            RuleFor(x => x.Quantity)
                .ApplyQuantityRules();
        }
    }

    /// <summary>Validates that cart parameters are not null, then applies CartParametersValidator rules.</summary>
    public static IRuleBuilderOptions<T, CartParameters> ApplyCartParametersRules<T>(
       this IRuleBuilder<T, CartParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new CartParametersValidator());
    }
}
