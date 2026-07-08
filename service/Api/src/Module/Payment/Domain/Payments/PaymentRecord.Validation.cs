namespace Module.Payment.Domain.Payments;

public static class PaymentValidation
{
    public static IRuleBuilderOptions<T, decimal> ApplyAmountRules<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithErrorCode(PaymentResult.Failure.AmountMustBePositive.Code)
            .WithMessage(PaymentResult.Failure.AmountMustBePositive.Description);
    }

    public static IRuleBuilderOptions<T, PaymentRecordState> ApplyStateTransitionRules<T>(
        this IRuleBuilder<T, PaymentRecordState> ruleBuilder,
        PaymentRecordState currentState)
    {
        return ruleBuilder
            .Must(target => IsValidTransition(currentState, target))
            .WithErrorCode(PaymentResult.Failure.InvalidStateTransition(currentState, currentState).Code)
            .WithMessage($"Invalid state transition from '{currentState}'.");
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