namespace Module.Payment.Domain.PaymentCaptures;

public static class PaymentCaptureValidation
{
    public static IRuleBuilderOptions<T, decimal> ApplyAmountRules<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithErrorCode(PaymentCaptureResult.Failure.AmountMustBePositive.Code)
            .WithMessage(PaymentCaptureResult.Failure.AmountMustBePositive.Message);
    }

    public static IRuleBuilderOptions<T, PaymentRecordState> ApplyStateTransitionRules<T>(
        this IRuleBuilder<T, PaymentRecordState> ruleBuilder,
        PaymentRecordState currentState)
    {
        return ruleBuilder
            .Must(target => IsValidTransition(currentState, target))
            .WithErrorCode(PaymentCaptureResult.Failure.InvalidStateTransition(currentState, currentState).Code)
            .WithMessage(PaymentCaptureResult.Failure.InvalidStateTransition(currentState, currentState).Message);
    }

    private static bool IsValidTransition(PaymentRecordState from, PaymentRecordState to) => (from, to) switch
    {
        (PaymentRecordState.Checkout, PaymentRecordState.Processing) => true,
        (PaymentRecordState.Processing, PaymentRecordState.Pending) => true,
        (PaymentRecordState.Processing, PaymentRecordState.Completed) => true,
        (PaymentRecordState.Processing, PaymentRecordState.Failed) => true,
        (PaymentRecordState.Processing, PaymentRecordState.Void) => true,
        (PaymentRecordState.Pending, PaymentRecordState.Completed) => true,
        (PaymentRecordState.Pending, PaymentRecordState.Failed) => true,
        (PaymentRecordState.Pending, PaymentRecordState.Void) => true,
        (PaymentRecordState.Failed, PaymentRecordState.Invalid) => true,
        (PaymentRecordState.Void, PaymentRecordState.Invalid) => true,
        _ => false
    };
}