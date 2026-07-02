using Module.Location.Domain.States;

namespace Module.Location.Features.Admin.States.Shared.Validators;

public static partial class StateValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyStateAbbreviationRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(errorCode: StateResult.Failure.AbbreviationRequired.Code)
            .WithMessage(errorMessage: StateResult.Failure.AbbreviationRequired.Message)
            .MaximumLength(maximumLength: StateConstant.Constraints.MaxAbbreviationLength)
            .WithErrorCode(errorCode: StateResult.Failure.AbbreviationTooLong.Code)
            .WithMessage(errorMessage: StateResult.Failure.AbbreviationTooLong.Message);
    }
}