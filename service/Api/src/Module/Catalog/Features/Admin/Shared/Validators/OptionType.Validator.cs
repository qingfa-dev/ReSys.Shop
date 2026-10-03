using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Shared.Validators;

public static class OptionTypeValidator
{
    public sealed class OptionTypeParametersValidator : AbstractValidator<OptionTypeParameters>
    {
        public OptionTypeParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Presentation).ApplyPresentationRules();
            RuleFor(x => x.Position).ApplyPositionRules();
        }
    }

    public static IRuleBuilderOptions<T, OptionTypeParameters> ApplyOptionTypeParametersRules<T>(
        this IRuleBuilder<T, OptionTypeParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new OptionTypeParametersValidator());
    }
}
