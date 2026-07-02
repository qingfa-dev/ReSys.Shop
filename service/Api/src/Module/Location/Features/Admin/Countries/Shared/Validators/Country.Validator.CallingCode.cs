using Module.Location.Domain.Countries;

namespace Module.Location.Features.Admin.Countries.Shared.Validators;

public static partial class CountryValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyCallingCodeRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(maximumLength: CountryConstant.Constraints.MaxCallingCodeLength)
            .WithErrorCode(errorCode: CountryResult.Failure.CallingCodeTooLong.Code)
            .WithMessage(errorMessage: CountryResult.Failure.CallingCodeTooLong.Message);
    }
}