using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Models;

namespace Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Validators;

public static partial class OptionValueValidators
{
    public sealed class OptionValueParametersValidator : AbstractValidator<OptionValueParameters>
    {
        public OptionValueParametersValidator()
        {
            RuleFor(x => x.OptionTypeId).ApplyOptionTypeIdRules();
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Presentation).ApplyPresentationRules();
            RuleFor(x => x.Position).ApplyPositionRules();
        }
    }

    public static IRuleBuilderOptions<T, OptionValueParameters> ApplyOptionValueParametersRules<T>(
        this IRuleBuilder<T, OptionValueParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new OptionValueParametersValidator());
    }
}