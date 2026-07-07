namespace Module.Promotions.Domain.CouponCodes;

public static class CouponCodeValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(CouponCodeResult.Errors.CodeRequired.Code)
            .WithMessage(CouponCodeResult.Errors.CodeRequired.Description)
            .MaximumLength(CouponCodeConstant.Constraints.MaxCodeLength)
            .WithErrorCode(CouponCodeResult.Errors.CodeTooLong.Code)
            .WithMessage(CouponCodeResult.Errors.CodeTooLong.Description);
    }

    public static IRuleBuilderOptions<T, CouponCodeState> ApplyStateRules<T>(this IRuleBuilder<T, CouponCodeState> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(CouponCodeResult.Errors.InvalidState.Code)
            .WithMessage(CouponCodeResult.Errors.InvalidState.Description);
    }
}