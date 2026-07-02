using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Models;

namespace Module.Location.Features.Admin.States.Shared.Validators;

public static partial class StateValidator
{
    public sealed class StateParametersValidator : AbstractValidator<StateParameters>
    {
        public StateParametersValidator()
        {
            RuleFor(expression: x => x.Name).ApplyStateNameRules();
            RuleFor(expression: x => x.Abbreviation).ApplyStateAbbreviationRules();
            RuleFor(expression: x => x.CountryId).NotEmpty()
                .WithErrorCode(errorCode: StateResult.Errors.CountryRequired.Code)
                .WithMessage(errorMessage: StateResult.Errors.CountryRequired.Message);
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