using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Models;

namespace Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Validators;

public static partial class OptionValueValidators
{
    public sealed class OptionValueRequestValidator : AbstractValidator<OptionValueRequest>
    {
        public OptionValueRequestValidator()
        {
            RuleFor(x => x.OptionTypeId).ApplyOptionTypeIdRules();
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Presentation).ApplyPresentationRules();
            RuleFor(x => x.Position).ApplyPositionRules();
        }
    }

    public static IRuleBuilderOptions<T, OptionValueRequest> ApplyOptionValueRequestRules<T>(
        this IRuleBuilder<T, OptionValueRequest> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new OptionValueRequestValidator());
    }
}