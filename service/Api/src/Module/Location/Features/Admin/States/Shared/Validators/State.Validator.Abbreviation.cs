using Module.Location.Domain.States;

namespace Module.Location.Features.Admin.States.Shared.Validators;

public static partial class StateValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyStateAbbreviationRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(errorCode: StateResult.Errors.AbbreviationRequired.Code)
            .WithMessage(errorMessage: StateResult.Errors.AbbreviationRequired.Message)
            .MaximumLength(maximumLength: StateConstant.Constraints.MaxAbbreviationLength)
            .WithErrorCode(errorCode: StateResult.Errors.AbbreviationTooLong.Code)
            .WithMessage(errorMessage: StateResult.Errors.AbbreviationTooLong.Message);
    }
}