namespace Module.Ordering.Domain.Orders;

public static class PaymentRecordValidation
{
    public static IRuleBuilderOptions<T, string> ApplyStateRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(state => state is "checkout" or "pending" or "completed" or "failed" or "void" or "invalid")
            .WithErrorCode(PaymentRecordResult.Errors.InvalidState.Code)
            .WithMessage(PaymentRecordResult.Errors.InvalidState.Description);
    }
}
