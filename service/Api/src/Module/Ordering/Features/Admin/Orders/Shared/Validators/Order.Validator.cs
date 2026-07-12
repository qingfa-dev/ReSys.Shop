using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.Shared.Validators;

public static partial class OrderValidator
{
    /// <summary>Validates shared order parameters — currency code and optional email format.</summary>
    public sealed class OrderParametersValidator : AbstractValidator<OrderParameters>
    {
        public OrderParametersValidator()
        {
            // Validate: Currency must be a non-empty ISO code within max length.
            RuleFor(x => x.Currency)
                .NotEmpty()
                .MaximumLength(OrderConstant.Constraints.MaxCurrencyLength)
                .WithErrorCode("Order.Currency.Invalid")
                .WithMessage($"Currency must be a valid ISO code (max {OrderConstant.Constraints.MaxCurrencyLength} chars).");

            // Validate: Email format checked only when a value is provided.
            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Email))
                .WithErrorCode("Order.Email.Invalid")
                .WithMessage("Email address is not valid.");
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
