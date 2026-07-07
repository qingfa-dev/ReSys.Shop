using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.Shared.Validators;

public static partial class OrderValidator
{
    // Validate: Order parameters rules
    public sealed class OrderParametersValidator : AbstractValidator<OrderParameters>
    {
        public OrderParametersValidator()
        {
            RuleFor(x => x.Currency)
                .NotEmpty()
                .MaximumLength(OrderConstant.Constraints.MaxCurrencyLength)
                .WithErrorCode("Order.Currency.Invalid")
                .WithMessage($"Currency must be a valid ISO code (max {OrderConstant.Constraints.MaxCurrencyLength} chars).");

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Email))
                .WithErrorCode("Order.Email.Invalid")
                .WithMessage("Email address is not valid.");
        }
    }

    public static IRuleBuilderOptions<T, OrderParameters> ApplyOrderParametersRules<T>(
        this IRuleBuilder<T, OrderParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new OrderParametersValidator());
    }
}
