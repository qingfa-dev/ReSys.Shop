using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Validators;

public static partial class CartValidator
{
    public sealed class CartParametersValidator : AbstractValidator<CartParameters>
    {
        public CartParametersValidator()
        {
            RuleFor(x => x.VariantId)
                .NotEmpty()
                .WithErrorCode(OrderResult.Errors.IdRequired.Code)
                .WithMessage(OrderResult.Errors.IdRequired.Message);
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithErrorCode(OrderResult.Errors.QuantityNotPositive.Code)
                .WithMessage(OrderResult.Errors.QuantityNotPositive.Message);
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