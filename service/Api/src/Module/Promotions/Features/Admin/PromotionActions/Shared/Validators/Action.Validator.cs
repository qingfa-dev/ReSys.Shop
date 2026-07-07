using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Features.Admin.PromotionActions.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionActions.Shared.Validators;

public static class PromotionActionValidator
{
    public sealed class PromotionActionParameterValidator : AbstractValidator<PromotionActionParameters>
    {
        public PromotionActionParameterValidator()
        {
            RuleFor(x => x.Type).ApplyTypeRules();
        }
    }

    public static IRuleBuilderOptions<T, PromotionActionParameters> ApplyPromotionActionParameterRules<T>(
        this IRuleBuilder<T, PromotionActionParameters> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new PromotionActionParameterValidator());
    }
}
