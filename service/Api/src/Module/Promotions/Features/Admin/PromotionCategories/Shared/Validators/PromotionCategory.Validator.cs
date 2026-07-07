using FluentValidation;
using Module.Promotions.Domain.PromotionCategories;
using Module.Promotions.Features.Admin.PromotionCategories.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionCategories.Shared.Validators;

/// <summary>Shared validators for promotion category models.</summary>
public static class PromotionCategoryValidator
{
    public sealed class PromotionCategoryParametersValidator : AbstractValidator<PromotionCategoryParameters>
    {
        public PromotionCategoryParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
        }
    }

    public static IRuleBuilderOptions<T, PromotionCategoryParameters> ApplyPromotionCategoryParametersRules<T>(
        this IRuleBuilder<T, PromotionCategoryParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new PromotionCategoryParametersValidator());
    }
}
