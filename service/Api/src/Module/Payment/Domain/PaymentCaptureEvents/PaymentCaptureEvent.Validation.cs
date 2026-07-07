namespace Module.Payment.Domain.PaymentCaptureEvents;

public static class PaymentCaptureEventValidation
{
    public static IRuleBuilderOptions<T, decimal> ApplyAmountRules<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithErrorCode(PaymentCaptureEventResult.Errors.InvalidAmount.Code)
            .WithMessage(PaymentCaptureEventResult.Errors.InvalidAmount.Description);
    }
}