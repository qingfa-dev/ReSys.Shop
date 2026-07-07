using FluentValidation;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.Promotions.Shared.Models;

namespace Module.Promotions.Features.Admin.Promotions.Shared.Validators;

/// <summary>Shared validators for promotion models.</summary>
public static class PromotionValidator
{
    public sealed class PromotionParametersValidator : AbstractValidator<PromotionParameters>
    {
        public PromotionParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Code).ApplyCodeRules();
            RuleFor(x => x.Description).ApplyDescriptionRules();
            RuleFor(x => x.Path).ApplyPathRules();
            RuleFor(x => x.MatchPolicy).ApplyMatchPolicyRules();
            RuleFor(x => x.Kind).ApplyKindRules();
        }
    }

    public static IRuleBuilderOptions<T, PromotionParameters> ApplyPromotionParametersRules<T>(
        this IRuleBuilder<T, PromotionParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new PromotionParametersValidator());
    }
}
