using FluentValidation;
using Shared.Application.Domain.Currencies;

namespace Module.Billing.Domain.PaymentCaptures;

// CAT-1 Validate: FluentValidation rule extensions for PaymentCapture fields
public static class PaymentCaptureValidation
{
    // Validate: Amount — must be positive
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
            .WithMessage((_, target) => PaymentCaptureResult.Failure.InvalidStateTransition(currentState, target).Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyNumberRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PaymentCaptureResult.Failure.NumberRequired.Code)
            .WithMessage(PaymentCaptureResult.Failure.NumberRequired.Message)
            .MaximumLength(PaymentConstant.Constraints.MaxNumberLength)
            .WithErrorCode(PaymentCaptureResult.Failure.NumberTooLong.Code)
            .WithMessage(PaymentCaptureResult.Failure.NumberTooLong.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyOrderIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PaymentCaptureResult.Failure.OrderIdRequired.Code)
            .WithMessage(PaymentCaptureResult.Failure.OrderIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyPaymentMethodIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PaymentCaptureResult.Failure.PaymentMethodIdRequired.Code)
            .WithMessage(PaymentCaptureResult.Failure.PaymentMethodIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyProviderKeyRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PaymentCaptureResult.Failure.ProviderKeyRequired.Code)
            .WithMessage(PaymentCaptureResult.Failure.ProviderKeyRequired.Message)
            .MaximumLength(PaymentConstant.Constraints.MaxProviderKeyLength)
            .WithErrorCode(PaymentCaptureResult.Failure.ProviderKeyTooLong.Code)
            .WithMessage(PaymentCaptureResult.Failure.ProviderKeyTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyResponseCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PaymentConstant.Constraints.MaxResponseCodeLength)
            .WithErrorCode(PaymentCaptureResult.Failure.ResponseCodeTooLong.Code)
            .WithMessage(PaymentCaptureResult.Failure.ResponseCodeTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAvsResponseRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PaymentConstant.Constraints.MaxAvsResponseLength)
            .WithErrorCode(PaymentCaptureResult.Failure.AvsResponseTooLong.Code)
            .WithMessage(PaymentCaptureResult.Failure.AvsResponseTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCvvCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PaymentConstant.Constraints.MaxCvvCodeLength)
            .WithErrorCode(PaymentCaptureResult.Failure.CvvCodeTooLong.Code)
            .WithMessage(PaymentCaptureResult.Failure.CvvCodeTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCvvMessageRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PaymentConstant.Constraints.MaxCvvMessageLength)
            .WithErrorCode(PaymentCaptureResult.Failure.CvvMessageTooLong.Code)
            .WithMessage(PaymentCaptureResult.Failure.CvvMessageTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyIntentClientSecretRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PaymentConstant.Constraints.MaxIntentClientSecretLength)
            .WithErrorCode(PaymentCaptureResult.Failure.ClientSecretTooLong.Code)
            .WithMessage(PaymentCaptureResult.Failure.ClientSecretTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplySourceTypeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PaymentCaptureResult.Failure.SourceTypeRequired.Code)
            .WithMessage(PaymentCaptureResult.Failure.SourceTypeRequired.Message)
            .MaximumLength(PaymentConstant.Constraints.MaxSourceTypeLength)
            .WithErrorCode(PaymentCaptureResult.Failure.SourceTypeTooLong.Code)
            .WithMessage(PaymentCaptureResult.Failure.SourceTypeTooLong.Message);
    }

    public static IRuleBuilderOptions<T, decimal> ApplyRefundedAmountRules<T>(this IRuleBuilder<T, decimal> ruleBuilder, decimal maxAmount)
    {
        return ruleBuilder
            .InclusiveBetween(PaymentConstant.RefundedAmount.MinValue, maxAmount)
            .WithErrorCode(PaymentCaptureResult.Failure.RefundAmountExceedsCaptured.Code)
            .WithMessage(PaymentCaptureResult.Failure.RefundAmountExceedsCaptured.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCurrencyRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PaymentCaptureResult.Failure.CurrencyRequired.Code)
            .WithMessage(PaymentCaptureResult.Failure.CurrencyRequired.Message)
            .Length(SystemCurrencyConstant.Constraints.MaxCodeLength)
            .WithErrorCode(PaymentCaptureResult.Failure.CurrencyInvalid.Code)
            .WithMessage(PaymentCaptureResult.Failure.CurrencyInvalid.Message);
    }

    private static bool IsValidTransition(PaymentRecordState from, PaymentRecordState to) => (from, to) switch
    {
        (PaymentRecordState.Checkout, PaymentRecordState.Processing) => true,
        (PaymentRecordState.Checkout, PaymentRecordState.Failed) => true,
        (PaymentRecordState.Checkout, PaymentRecordState.Disputed) => true,
        (PaymentRecordState.Processing, PaymentRecordState.Pending) => true,
        (PaymentRecordState.Processing, PaymentRecordState.Completed) => true,
        (PaymentRecordState.Processing, PaymentRecordState.Failed) => true,
        (PaymentRecordState.Processing, PaymentRecordState.Void) => true,
        (PaymentRecordState.Processing, PaymentRecordState.Disputed) => true,
        (PaymentRecordState.Pending, PaymentRecordState.Completed) => true,
        (PaymentRecordState.Pending, PaymentRecordState.Failed) => true,
        (PaymentRecordState.Pending, PaymentRecordState.Void) => true,
        (PaymentRecordState.Pending, PaymentRecordState.Disputed) => true,
        (PaymentRecordState.Completed, PaymentRecordState.Disputed) => true,
        (PaymentRecordState.Failed, PaymentRecordState.Disputed) => true,
        (PaymentRecordState.Failed, PaymentRecordState.Invalid) => true,
        (PaymentRecordState.Void, PaymentRecordState.Invalid) => true,
        _ => false
    };
}