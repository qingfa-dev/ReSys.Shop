namespace Module.Payment.Domain.RefundReasons;

public static class RefundReasonValidation
{
    public static IRuleBuilderOptions<T, string> ApplyNameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(RefundReasonResult.Errors.NameRequired.Code)
            .WithMessage(RefundReasonResult.Errors.NameRequired.Description);
    }
}
