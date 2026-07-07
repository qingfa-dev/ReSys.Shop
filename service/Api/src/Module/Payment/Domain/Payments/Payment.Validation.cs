namespace Module.Payment.Domain.Payments;

public static class PaymentValidation
{
    public static IRuleBuilderOptions<T, decimal> ApplyAmountRules<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithErrorCode(PaymentResult.Errors.AmountMustBePositive.Code)
            .WithMessage(PaymentResult.Errors.AmountMustBePositive.Description);
    }

    public static IRuleBuilderOptions<T, PaymentState> ApplyStateTransitionRules<T>(
        this IRuleBuilder<T, PaymentState> ruleBuilder,
        PaymentState currentState)
    {
        return ruleBuilder
            .Must(target => IsValidTransition(currentState, target))
            .WithErrorCode(PaymentResult.Errors.InvalidStateTransition(currentState, currentState).Code)
            .WithMessage($"Invalid state transition from '{currentState}'.");
    }

    private static bool IsValidTransition(PaymentState from, PaymentState to) => (from, to) switch
    {
        (PaymentState.Checkout, PaymentState.Processing) => true,
        (PaymentState.Processing, PaymentState.Pending) => true,
        (PaymentState.Processing, PaymentState.Completed) => true,
        (PaymentState.Processing, PaymentState.Failed) => true,
        (PaymentState.Processing, PaymentState.Void) => true,
        (PaymentState.Pending, PaymentState.Completed) => true,
        (PaymentState.Pending, PaymentState.Failed) => true,
        (PaymentState.Pending, PaymentState.Void) => true,
        (PaymentState.Failed, PaymentState.Invalid) => true,
        (PaymentState.Void, PaymentState.Invalid) => true,
        _ => false
    };
}