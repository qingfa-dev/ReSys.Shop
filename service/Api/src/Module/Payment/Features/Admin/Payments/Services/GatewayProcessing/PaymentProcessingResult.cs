using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Features.Admin.Payments.Services.GatewayProcessing;

public sealed record PaymentProcessingResult
{
    public string? Message { get; init; }
    public PaymentRecordState? State { get; init; }
    public decimal? CapturedAmount { get; init; }
    public decimal? RefundedAmount { get; init; }
    public bool CaptureEventCreated { get; init; }
}

public static class ProcessingResult
{
    public static class Success
    {
        public static PaymentProcessingResult Processed(string number) => new()
        {
            Message = $"Payment '{number}' was successfully processed.",
            State = PaymentRecordState.Processing
        };

        public static PaymentProcessingResult Pended(string number) => new()
        {
            Message = $"Payment '{number}' was successfully pended.",
            State = PaymentRecordState.Pending
        };

        public static PaymentProcessingResult Completed(string number) => new()
        {
            Message = $"Payment '{number}' was successfully completed.",
            State = PaymentRecordState.Completed
        };

        public static PaymentProcessingResult Captured(string number, decimal amount) => new()
        {
            Message = $"Payment '{number}' was captured for {amount}.",
            State = PaymentRecordState.Completed,
            CapturedAmount = amount
        };

        public static PaymentProcessingResult Voided(string number) => new()
        {
            Message = $"Payment '{number}' was successfully voided.",
            State = PaymentRecordState.Void
        };

        public static PaymentProcessingResult Credited(string number, decimal amount) => new()
        {
            Message = $"Payment '{number}' was credited for {amount}.",
            RefundedAmount = amount
        };

        public static PaymentProcessingResult ConfirmCompleted(string number) => new()
        {
            Message = $"Payment '{number}' was confirmed and completed.",
            State = PaymentRecordState.Completed
        };

        public static PaymentProcessingResult ConfirmPended(string number) => new()
        {
            Message = $"Payment '{number}' was confirmed and pended.",
            State = PaymentRecordState.Pending
        };
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
