// Context: Storefront API route constants and descriptions — consumed by Endpoint files
namespace Module.Billing.Features.Shared;

public sealed partial class BillingFeature
{
    public static class Storefront
    {
        public static class Payments
        {
            public static class CreateIntent
            {
                public const string Route = "api/storefront/cart/payment/intent";
                public const string Description = "Create a payment intent for the current cart";
                public const string Summary = "Create payment intent";
            }

            public static class Confirm
            {
                public const string Route = "api/storefront/cart/payment/intent/{paymentId:guid}/confirm";
                public const string Description = "Confirm a payment for the current cart";
                public const string Summary = "Confirm payment";
            }

            public static class Status
            {
                public const string Route = "api/storefront/cart/payment/intent/{orderId:guid}";
                public const string Description = "Retrieve payment status for an order";
                public const string Summary = "Get payment status";
            }
        }

        public static class PaymentMethods
        {
            public static class GetAll
            {
                public const string Route = "api/storefront/billing/payment-methods";
                public const string Description = "Retrieve available payment methods";
                public const string Summary = "Get payment methods";
            }

            public static class SetupIntent
            {
                public const string Route = "api/storefront/billing/payment-methods/setup-intent";
                public const string Description = "Create a Stripe SetupIntent for saving payment methods";
                public const string Summary = "Create setup intent";
            }
        }

        public static class Webhooks
        {
            public static class Stripe
            {
                public const string Route = "api/storefront/billing/webhooks/stripe";
                public const string Description = "Receive Stripe webhook events for payment processing";
                public const string Summary = "Stripe webhook receiver";
            }
        }
    }
}
