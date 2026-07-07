namespace Module.Promotions.Domain.OrderPromotions;

public static class OrderPromotionValidation
{
    public static IRuleBuilderOptions<T, System.Guid> ApplyOrderIdRules<T>(this IRuleBuilder<T, System.Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OrderPromotionResult.Errors.OrderRequired.Code)
            .WithMessage(OrderPromotionResult.Errors.OrderRequired.Description);
    }

    public static IRuleBuilderOptions<T, System.Guid> ApplyPromotionIdRules<T>(this IRuleBuilder<T, System.Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OrderPromotionResult.Errors.PromotionRequired.Code)
            .WithMessage(OrderPromotionResult.Errors.PromotionRequired.Description);
    }
}
