namespace Module.Promotions.Domain.PromotionActions;

public static class PromotionActionValidation
{
    public static IRuleBuilderOptions<T, string> ApplyTypeRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PromotionActionResult.Errors.TypeRequired.Code)
            .WithMessage(PromotionActionResult.Errors.TypeRequired.Description);
    }
}
