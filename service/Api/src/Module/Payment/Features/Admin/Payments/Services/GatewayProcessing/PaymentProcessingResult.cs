using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Features.Admin.Payments.Services.GatewayProcessing;

public sealed record PaymentProcessingResult
{
    public PaymentRecordState? State { get; init; }
    public decimal? CapturedAmount { get; init; }
    public decimal? RefundedAmount { get; init; }
    public bool CaptureEventCreated { get; init; }
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
        public static Error InvalidStateTransition(PaymentRecordState from, PaymentRecordState to) => Error.Validation(
            code: "Payment.State.InvalidTransition",
            message: $"Cannot transition payment from '{from}' to '{to}'.");

        public static Error AlreadyCompleted => Error.Conflict(
            code: "Payment.AlreadyCompleted",
            message: "Payment has already been completed.");

        public static Error AlreadyVoided => Error.Conflict(
            code: "Payment.AlreadyVoided",
            message: "Payment has already been voided.");

        public static Error AmountExceedsAuthorized => Error.Validation(
            code: "Payment.Amount.ExceedsAuthorized",
            message: "Capture amount exceeds the authorized amount.");

        public static Error ProcessingSourceRequired => Error.Validation(
            code: "Payment.Processing.SourceRequired",
            message: "Payment source is required but was not provided.");

        public static Error ProcessingAlreadyProcessing => Error.Conflict(
            code: "Payment.Processing.AlreadyProcessing",
            message: "Payment is already being processed.");

        public static Error CreditNotAllowed => Error.Conflict(
            code: "Payment.Credit.NotAllowed",
            message: "Payment is not in a completed state and cannot be credited.");

        public static Error GatewayDeclined(string detail) => Error.BadRequest(
            code: "Payment.Gateway.Declined",
            message: detail);
    }
}
