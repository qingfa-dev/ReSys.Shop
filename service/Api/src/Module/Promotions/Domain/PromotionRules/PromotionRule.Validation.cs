namespace Module.Promotions.Domain.PromotionRules;

public static class PromotionRuleValidation
{
    public static IRuleBuilderOptions<T, string> ApplyTypeRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PromotionRuleResult.Errors.TypeRequired.Code)
            .WithMessage(PromotionRuleResult.Errors.TypeRequired.Description);
    }
}
