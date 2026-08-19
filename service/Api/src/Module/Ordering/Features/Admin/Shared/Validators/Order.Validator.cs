using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Models;

namespace Module.Ordering.Features.Admin.Shared.Validators;

public static partial class OrderValidator
{
    /// <summary>Validates shared order parameters — currency code and optional email format.</summary>
    public sealed class OrderParametersValidator : AbstractValidator<OrderParameters>
    {
        public OrderParametersValidator()
        {
            // Validate: Currency must be a non-empty ISO code within max length.
            RuleFor(x => x.Currency).ApplyCurrencyRules();

            // Validate: Email format checked only when a value is provided.
            RuleFor(x => x.Email)
                .ApplyEmailRules()
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }

    /// <summary>
    /// Applies shared order parameters validation rules to a command's request property.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the OrderParameters property.</param>
    /// <returns>The rule builder options for chaining.</returns>
    public static IRuleBuilderOptions<T, OrderParameters> ApplyOrderParametersRules<T>(
        this IRuleBuilder<T, OrderParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new OrderParametersValidator());
    }
}
