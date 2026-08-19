using Module.Location.Domain.States;

namespace Module.Location.Features.Shared.States.Validators;

public static partial class StateValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyStateNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(errorCode: StateResult.Failure.NameRequired.Code)
            .WithMessage(errorMessage: StateResult.Failure.NameRequired.Message)
            .MaximumLength(maximumLength: StateConstant.Constraints.MaxNameLength)
            .WithErrorCode(errorCode: StateResult.Failure.NameTooLong.Code)
            .WithMessage(errorMessage: StateResult.Failure.NameTooLong.Message);
    }
}