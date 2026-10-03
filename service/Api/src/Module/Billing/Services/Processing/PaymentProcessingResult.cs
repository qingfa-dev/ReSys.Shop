using Module.Billing.Domain.PaymentCaptures;

namespace Module.Billing.Services.Processing;

public sealed record PaymentProcessingResult
{
    public PaymentRecordState? State { get; init; }
    public decimal? CapturedAmount { get; init; }
    public decimal? RefundedAmount { get; init; }
}

public static class ProcessingResult
{
    public static class Success
    {
        public static Result<PaymentProcessingResult> Processed(string number) =>
            Result<PaymentProcessingResult>.Ok(
                new PaymentProcessingResult { State = PaymentRecordState.Processing },
                message: $"Payment '{number}' was successfully processed.");

        public static Result<PaymentProcessingResult> Pended(string number) =>
            Result<PaymentProcessingResult>.Ok(
                new PaymentProcessingResult { State = PaymentRecordState.Pending },
                message: $"Payment '{number}' was successfully pended.");

        public static Result<PaymentProcessingResult> Completed(string number) =>
            Result<PaymentProcessingResult>.Ok(
                new PaymentProcessingResult { State = PaymentRecordState.Completed },
                message: $"Payment '{number}' was successfully completed.");

        public static Result<PaymentProcessingResult> Captured(string number, decimal amount) =>
            Result<PaymentProcessingResult>.Ok(
                new PaymentProcessingResult { State = PaymentRecordState.Completed, CapturedAmount = amount },
                message: $"Payment '{number}' was captured for {amount}.");

        public static Result<PaymentProcessingResult> Voided(string number) =>
            Result<PaymentProcessingResult>.Ok(
                new PaymentProcessingResult { State = PaymentRecordState.Void },
                message: $"Payment '{number}' was successfully voided.");

        public static Result<PaymentProcessingResult> Credited(string number, decimal amount) =>
            Result<PaymentProcessingResult>.Ok(
                new PaymentProcessingResult { RefundedAmount = amount },
                message: $"Payment '{number}' was credited for {amount}.");

        public static Result<PaymentProcessingResult> ConfirmCompleted(string number) =>
            Result<PaymentProcessingResult>.Ok(
                new PaymentProcessingResult { State = PaymentRecordState.Completed },
                message: $"Payment '{number}' was confirmed and completed.");

        public static Result<PaymentProcessingResult> ConfirmPended(string number) =>
            Result<PaymentProcessingResult>.Ok(
                new PaymentProcessingResult { State = PaymentRecordState.Pending },
                message: $"Payment '{number}' was confirmed and pended.");
    }

    public static class Errors
    {
        public static Error InvalidStateTransition(PaymentRecordState from, PaymentRecordState to)
            => PaymentCaptureResult.Failure.InvalidStateTransition(from, to);

        public static Error AlreadyCompleted
            => PaymentCaptureResult.Failure.AlreadyCompleted;

        public static Error AlreadyVoided
            => PaymentCaptureResult.Failure.AlreadyVoided;

        public static Error AmountExceedsAuthorized
            => PaymentCaptureResult.Failure.AmountExceedsAuthorized;

        public static Error ProcessingSourceRequired
            => PaymentCaptureResult.Failure.ProcessingSourceRequired;

        public static Error ProcessingAlreadyProcessing
            => PaymentCaptureResult.Failure.ProcessingAlreadyProcessing;

        public static Error CreditNotAllowed
            => PaymentCaptureResult.Failure.CreditNotAllowed;

        public static Error GatewayDeclined(string detail) => Error.BadRequest(
            code: "Payment.Gateway.Declined",
            message: detail);
    }
}