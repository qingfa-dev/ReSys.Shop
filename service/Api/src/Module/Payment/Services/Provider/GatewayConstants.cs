using Shared.Application.Domain.Currencies;

namespace Module.Payment.Services.Provider;

/// <summary>Gateway-level contract values: provider keys, Stripe/Bogus domain strings, webhook events, response messages, error codes.</summary>
// Context: Shared constants used across Provider, Processing, Webhook, and Feature layers
public static class GatewayConstants
{
    public static class Providers
    {
        public const string Stripe = "stripe";
        public const string Bogus = "bogus";
    }

    public static class Currency
    {
        public const string Usd = SystemCurrencyConstant.Defaults.Code;
    }

    public static class Idempotency
    {
        public const string Prefix = "shop-";
        public static string ForPayment(string paymentNumber) => $"{Prefix}{paymentNumber}";
    }

    public static class Metadata
    {
        public const string OrderIdKey = "order_id";
        public const string PaymentIdKey = "payment_id";
        public const string PaymentMethodIdKey = "payment_method_id";
    }

    public static class Configuration
    {
        public const string SectionName = "GatewayProviders";
        public const string SettingsEncryptionKey = "SettingsEncryptionKey";
    }

    public static class Stripe
    {
        public static class ConfirmationMethod
        {
            public const string Manual = "manual";
        }

        public static class CaptureMethod
        {
            public const string Automatic = "automatic";
            public const string Manual = "manual";
        }

        public static class IntentStatus
        {
            public const string RequiresPaymentMethod = "requires_payment_method";
            public const string RequiresConfirmation = "requires_confirmation";
            public const string RequiresAction = "requires_action";
            public const string Processing = "processing";
            public const string RequiresCapture = "requires_capture";
            public const string Canceled = "canceled";
            public const string Succeeded = "succeeded";
        }
    }

    public static class Bogus
    {
        public static class TestCards
        {
            public const string Success = "4242424242424242";
            public const string Declined = "4000000000000002";
            public const string InsufficientFunds = "4000000000009995";
        }

        public const string SetupIntentSecretPrefix = "pi_setup_fake_";
    }

    public static class Amounts
    {
        public const long CentsMultiplier = 100;
        public const decimal MaxSafeDollarAmount = 92_233_720_368_547_758.07m;
    }

    public static class Webhook
    {
        public static class Headers
        {
            public const string StripeSignature = "Stripe-Signature";
        }

        public static class Messages
        {
            public const string MissingSignature = "Missing Stripe-Signature header.";
            public const string InvalidSignature = "Invalid Stripe webhook signature.";
            public const string InvalidPayload = "Invalid Stripe webhook payload.";
        }
    }

    public static class WebhookEvents
    {
        public static class Stripe
        {
            public const string PaymentIntentSucceeded = "payment_intent.succeeded";
            public const string PaymentIntentPaymentFailed = "payment_intent.payment_failed";
            public const string ChargeRefunded = "charge.refunded";
            public const string ChargeDisputeCreated = "charge.dispute.created";
            public const string PaymentIntentRequiresAction = "payment_intent.requires_action";
            public const string PaymentIntentProcessing = "payment_intent.processing";
            public const string PaymentIntentCanceled = "payment_intent.canceled";
        }
    }

    public static class ResponseMessages
    {
        public const string PaymentCaptured = "Payment captured.";
        public const string Authorized = "Authorized.";
        public const string Captured = "Captured.";
        public const string Voided = "Voided.";
        public const string Refunded = "Refunded.";
    }

    public static class ErrorCodes
    {
        public static class Stripe
        {
            public const string CaptureMissingIntent = "Stripe.Capture.MissingIntent";
            public const string CreditMissingIntent = "Stripe.Credit.MissingIntent";
            public const string CancelMissingIntent = "Stripe.Cancel.MissingIntent";
            public const string UnknownError = "Stripe.UnknownError";
        }

        public static class Bogus
        {
            public const string CardDeclined = "Bogus.CardDeclined";
            public const string InsufficientFunds = "Bogus.InsufficientFunds";
            public const string UnknownCard = "Bogus.UnknownCard";
        }
    }
}