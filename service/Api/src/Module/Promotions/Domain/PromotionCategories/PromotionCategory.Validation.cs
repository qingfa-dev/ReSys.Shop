namespace Module.Promotions.Domain.PromotionCategories;

public static class PromotionCategoryValidation
{
    public static IRuleBuilderOptions<T, string> ApplyNameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PromotionCategoryResult.Errors.NameRequired.Code)
            .WithMessage(PromotionCategoryResult.Errors.NameRequired.Description);
    }
}
