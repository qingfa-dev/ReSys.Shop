using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Features.Admin.PromotionRules.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionRules.Shared.Validators;

public static class PromotionRuleValidator
{
    public sealed class PromotionRuleParameterValidator : AbstractValidator<PromotionRuleParameters>
    {
        public PromotionRuleParameterValidator()
        {
            RuleFor(x => x.Type).ApplyTypeRules();
        }
    }

    public static IRuleBuilderOptions<T, PromotionRuleParameters> ApplyPromotionRuleParameterRules<T>(
        this IRuleBuilder<T, PromotionRuleParameters> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new PromotionRuleParameterValidator());
    }
}
