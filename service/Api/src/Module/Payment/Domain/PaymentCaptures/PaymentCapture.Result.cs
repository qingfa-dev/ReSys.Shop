namespace Module.Payment.Domain.PaymentCaptures;

/// <summary>
/// Provides success messages and error definitions for payment domain operations.
/// </summary>
public static class PaymentCaptureResult
{
    /// <summary>
    /// Contains success message templates for payment lifecycle events.
    /// </summary>
    public static class Success
    {
        /// <summary>Returns a success message for a created payment.</summary>
        public static string Created(string number) => $"Payment '{number}' was successfully created.";
        /// <summary>Returns a success message for a processed payment.</summary>
        public static string Processed(string number) => $"Payment '{number}' was successfully processed.";
        /// <summary>Returns a success message for a pended payment.</summary>
        public static string Pended(string number) => $"Payment '{number}' was successfully pended.";
        /// <summary>Returns a success message for a completed payment.</summary>
        public static string Completed(string number) => $"Payment '{number}' was successfully completed.";
        /// <summary>Returns a success message indicating the payment was already completed (idempotent replay).</summary>
        public static string AlreadyCompleted(string number) => $"Payment '{number}' was already completed.";
        /// <summary>Returns a success message for a voided payment.</summary>
        public static string Voided(string number) => $"Payment '{number}' was successfully voided.";
        /// <summary>Returns a failure notification message.</summary>
        public static string Failed(string number) => $"Payment '{number}' was marked as failed.";
        /// <summary>Returns a success message for a captured amount on a payment.</summary>
        public static string Captured(string number, decimal amount) => $"Payment '{number}' was captured for {amount}.";
        /// <summary>Returns a success message for a credited amount on a payment.</summary>
        public static string Credited(string number, decimal amount) => $"Payment '{number}' was credited for {amount}.";
        /// <summary>Returns a success message for a refunded amount on a payment.</summary>
        public static string Refunded(string number, decimal amount) => $"Payment '{number}' was refunded for {amount}.";
    }

    /// <summary>
    /// Contains error definitions for payment validation and business rule violations.
    /// </summary>
    public static class Failure
    {
        #region Validation
        /// <summary>Provider is not registered in the gateway registry.</summary>
        public static Error ProviderNotRegistered(string providerKey) => Error.NotFound(
            code: "Payment.ProviderKey.NotRegistered",
            message: $"Provider '{providerKey}' is not registered in the gateway registry.");

        /// <summary>Error indicating the payment amount must be greater than zero.</summary>
        public static Error AmountMustBePositive => Error.Validation(
            code: "Payment.Amount.Positive",
            message: "Payment amount must be greater than zero.");

        /// <summary>Error indicating an invalid payment state transition was attempted.</summary>
        public static Error InvalidStateTransition(PaymentRecordState from, PaymentRecordState to) => Error.Validation(
            code: "Payment.State.InvalidTransition",
            message: $"Cannot transition payment from '{from}' to '{to}'.");
        #endregion

        #region Number
        /// <summary>Error indicating the payment number is required.</summary>
        public static Error NumberRequired => Error.Validation(
            code: "Payment.Number.Required",
            message: "Payment number is required.");

        /// <summary>Error indicating the payment number exceeds the maximum length.</summary>
        public static Error NumberTooLong => Error.Validation(
            code: "Payment.Number.TooLong",
            message: $"Payment number cannot exceed {PaymentConstant.Constraints.MaxNumberLength} characters.");

        /// <summary>Error indicating the payment number format is invalid.</summary>
        public static Error NumberInvalid => Error.Validation(
            code: "Payment.Number.Invalid",
            message: "Payment number format is invalid.");
        #endregion

        #region OrderId
        /// <summary>Error indicating the order identifier is required.</summary>
        public static Error OrderIdRequired => Error.Validation(
            code: "Payment.OrderId.Required",
            message: "Order identifier is required.");
        #endregion

        #region PaymentMethodId
        /// <summary>Error indicating the payment method identifier is required.</summary>
        public static Error PaymentMethodIdRequired => Error.Validation(
            code: "Payment.PaymentMethodId.Required",
            message: "Payment method identifier is required.");
        #endregion

        #region ProviderKey
        /// <summary>Error indicating the provider key is required.</summary>
        public static Error ProviderKeyRequired => Error.Validation(
            code: "Payment.ProviderKey.Required",
            message: "Provider key is required.");

        /// <summary>Error indicating the provider key exceeds the maximum length.</summary>
        public static Error ProviderKeyTooLong => Error.Validation(
            code: "Payment.ProviderKey.TooLong",
            message: $"Provider key cannot exceed {PaymentConstant.Constraints.MaxProviderKeyLength} characters.");
        #endregion

        #region SourceType
        /// <summary>Error indicating the source type is required.</summary>
        public static Error SourceTypeRequired => Error.Validation(
            code: "Payment.SourceType.Required",
            message: "Payment source type is required.");

        /// <summary>Error indicating the source type exceeds the maximum length.</summary>
        public static Error SourceTypeTooLong => Error.Validation(
            code: "Payment.SourceType.TooLong",
            message: $"Payment source type cannot exceed {PaymentConstant.Constraints.MaxSourceTypeLength} characters.");
        #endregion

        #region ResponseCode
        /// <summary>Error indicating the response code exceeds the maximum length.</summary>
        public static Error ResponseCodeTooLong => Error.Validation(
            code: "Payment.ResponseCode.TooLong",
            message: $"Response code cannot exceed {PaymentConstant.Constraints.MaxResponseCodeLength} characters.");
        #endregion

        #region AvsResponse
        /// <summary>Error indicating the AVS response exceeds the maximum length.</summary>
        public static Error AvsResponseTooLong => Error.Validation(
            code: "Payment.AvsResponse.TooLong",
            message: $"AVS response cannot exceed {PaymentConstant.Constraints.MaxAvsResponseLength} characters.");
        #endregion

        #region CvvCode
        /// <summary>Error indicating the CVV code exceeds the maximum length.</summary>
        public static Error CvvCodeTooLong => Error.Validation(
            code: "Payment.CvvCode.TooLong",
            message: $"CVV code cannot exceed {PaymentConstant.Constraints.MaxCvvCodeLength} characters.");
        #endregion

        #region CvvMessage
        /// <summary>Error indicating the CVV message exceeds the maximum length.</summary>
        public static Error CvvMessageTooLong => Error.Validation(
            code: "Payment.CvvMessage.TooLong",
            message: $"CVV message cannot exceed {PaymentConstant.Constraints.MaxCvvMessageLength} characters.");
        #endregion

        #region IntentClientSecret
        /// <summary>Error indicating the client secret exceeds the maximum length.</summary>
        public static Error ClientSecretTooLong => Error.Validation(
            code: "Payment.ClientSecret.TooLong",
            message: $"Client secret cannot exceed {PaymentConstant.Constraints.MaxIntentClientSecretLength} characters.");
        #endregion

        #region Currency
        /// <summary>Error indicating the currency code is required.</summary>
        public static Error CurrencyRequired => Error.Validation(
            code: "Payment.Currency.Required",
            message: "Currency code is required.");

        /// <summary>Error indicating the currency code is invalid.</summary>
        public static Error CurrencyInvalid => Error.Validation(
            code: "Payment.Currency.Invalid",
            message: "Currency code must be a valid ISO 4217 three-letter code.");
        #endregion

        #region Business
        /// <summary>Error indicating the payment was not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "Payment.NotFound",
            message: "Payment was not found.");

        /// <summary>Error indicating the payment has already been completed.</summary>
        public static Error AlreadyCompleted => Error.Conflict(
            code: "Payment.AlreadyCompleted",
            message: "Payment has already been completed.");

        /// <summary>Error indicating the payment has already been voided.</summary>
        public static Error AlreadyVoided => Error.Conflict(
            code: "Payment.AlreadyVoided",
            message: "Payment has already been voided.");

        /// <summary>Error indicating the payment has already been marked as failed.</summary>
        public static Error AlreadyFailed => Error.Conflict(
            code: "Payment.AlreadyFailed",
            message: "Payment has already been marked as failed.");

        /// <summary>Error indicating the capture amount exceeds the authorized amount.</summary>
        public static Error AmountExceedsAuthorized => Error.Validation(
            code: "Payment.Amount.ExceedsAuthorized",
            message: "Capture amount exceeds the authorized amount.");

        /// <summary>Error indicating the payment method is inactive and cannot process.</summary>
        public static Error PaymentMethodInactive => Error.Conflict(
            code: "Payment.PaymentMethod.Inactive",
            message: "Cannot process payment because the payment method is inactive.");

        /// <summary>Payment source is required but was not provided.</summary>
        public static Error ProcessingSourceRequired => Error.Validation(
            code: "Payment.Processing.SourceRequired",
            message: "Payment source is required but was not provided.");

        /// <summary>Payment is already being processed.</summary>
        public static Error ProcessingAlreadyProcessing => Error.Conflict(
            code: "Payment.Processing.AlreadyProcessing",
            message: "Payment is already being processed.");

        /// <summary>Payment is not in a completed state and cannot be credited.</summary>
        public static Error CreditNotAllowed => Error.Conflict(
            code: "Payment.Credit.NotAllowed",
            message: "Payment is not in a completed state and cannot be credited.");

        /// <summary>Payment has not succeeded at the gateway.</summary>
        public static Error NotSucceeded => Error.Validation(
            code: "Payment.Confirm.NotSucceeded",
            message: "Payment has not succeeded at the gateway.");

        /// <summary>Error indicating the refund amount exceeds the captured amount.</summary>
        public static Error RefundAmountExceedsCaptured => Error.Validation(
            code: "Payment.Refund.AmountExceedsCaptured",
            message: "Refund amount cannot exceed the captured amount.");

        /// <summary>Error indicating the order has an outstanding payment requirement.</summary>
        public static Error OrderPaymentRequired => Error.Conflict(
            code: "Payment.OrderPaymentRequired",
            message: "Order requires a payment before proceeding.");

        /// <summary>Error indicating the payment source was not found.</summary>
        public static Error SourceNotFound => Error.NotFound(
            code: "Payment.Source.NotFound",
            message: "Payment source was not found.");
        #endregion Business

        #region Gateway
        /// <summary>Error indicating the gateway request timed out.</summary>
        public static Error GatewayTimeout(string message) => Error.BadRequest(
            code: "Payment.Gateway.Timeout",
            message: message);

        /// <summary>Error indicating the gateway is unavailable.</summary>
        public static Error GatewayUnavailable(string message) => Error.BadRequest(
            code: "Payment.Gateway.Unavailable",
            message: message);
        #endregion Gateway

        #region Gateway Response Errors (dynamic message)
        /// <summary>Gateway capture action failed.</summary>
        public static Error CaptureFailed(string message) => Error.BadRequest(
            code: "Payment.Capture.Failed",
            message: message);

        /// <summary>Gateway void action failed.</summary>
        public static Error VoidFailed(string message) => Error.BadRequest(
            code: "Payment.Void.Failed",
            message: message);

        /// <summary>Gateway cancel action failed.</summary>
        public static Error CancelFailed(string message) => Error.BadRequest(
            code: "Payment.Cancel.Failed",
            message: message);

        /// <summary>Gateway credit action failed.</summary>
        public static Error CreditFailed(string message) => Error.BadRequest(
            code: "Payment.Credit.Failed",
            message: message);

        /// <summary>Gateway action returned a failure response.</summary>
        public static Error GatewayError(string message) => Error.BadRequest(
            code: "Payment.Gateway.Error",
            message: message);
        #endregion Gateway Response Errors
    }
}