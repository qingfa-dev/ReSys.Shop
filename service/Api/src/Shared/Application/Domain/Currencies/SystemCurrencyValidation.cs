using FluentValidation;

namespace Shared.Application.Domain.Currencies;

public static class SystemCurrencyValidation
{
    public static IRuleBuilderOptions<T, string> ApplyCurrencyRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(SystemCurrencyResult.CurrencyInvalid.Code)
            .WithMessage(SystemCurrencyResult.CurrencyInvalid.Message)
            .MaximumLength(SystemCurrencyConstant.Constraints.MaxCodeLength)
            .WithErrorCode(SystemCurrencyResult.CurrencyTooLong.Code)
            .WithMessage(SystemCurrencyResult.CurrencyTooLong.Message);
    }
}
