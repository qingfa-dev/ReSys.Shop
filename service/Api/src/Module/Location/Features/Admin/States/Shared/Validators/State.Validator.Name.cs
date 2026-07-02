using Module.Location.Domain.States;

namespace Module.Location.Features.Admin.States.Shared.Validators;

public static partial class StateValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyStateNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(errorCode: StateResult.Errors.NameRequired.Code)
            .WithMessage(errorMessage: StateResult.Errors.NameRequired.Message)
            .MaximumLength(maximumLength: StateConstant.Constraints.MaxNameLength)
            .WithErrorCode(errorCode: StateResult.Errors.NameTooLong.Code)
            .WithMessage(errorMessage: StateResult.Errors.NameTooLong.Message);
    }
}