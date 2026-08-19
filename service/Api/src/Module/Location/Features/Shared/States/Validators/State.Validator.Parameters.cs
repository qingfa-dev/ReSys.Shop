using Module.Location.Domain.States;
using Module.Location.Features.Shared.States.Models;

namespace Module.Location.Features.Shared.States.Validators;

public static partial class StateValidator
{
    public sealed class StateParametersValidator : AbstractValidator<StateParameters>
    {
        public StateParametersValidator()
        {
            RuleFor(expression: x => x.Name).ApplyStateNameRules();
            RuleFor(expression: x => x.Abbreviation).ApplyStateAbbreviationRules();
            RuleFor(expression: x => x.CountryId).NotEmpty()
                .WithErrorCode(errorCode: StateResult.Failure.CountryRequired.Code)
                .WithMessage(errorMessage: StateResult.Failure.CountryRequired.Message);
        }
    }

    public static IRuleBuilderOptions<T, StateParameters> ApplyStateParametersRules<T>(
        this IRuleBuilder<T, StateParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(validator: new StateParametersValidator());
    }
}