namespace Module.Payment.Features.Shared;

public sealed partial class PaymentFeature
{
    public static class Storefront
    {
        public const string Route = "api/storefront";

        public static class Payment
        {
            public const string BaseRoute = $"{Route}/payment";

            public static class CreateIntent
            {
                public const string Route = $"{BaseRoute}/create-intent";
                public const string Description = "Create a payment intent for an order";
                public const string Summary = "Create payment intent";
            }

            public static class Confirm
            {
                public const string Route = $"{BaseRoute}/confirm/{{paymentId:guid}}";
                public const string Description = "Confirm a payment";
                public const string Summary = "Confirm payment";
            }

            public static class Methods
            {
                public const string Route = $"{BaseRoute}/methods";
                public const string Description = "Retrieve available payment methods";
                public const string Summary = "Get payment methods";
            }

            public static class SetupIntent
            {
                public const string Route = $"{BaseRoute}/setup-intent";
                public const string Description = "Create a Stripe SetupIntent for saving payment methods";
                public const string Summary = "Create setup intent";
            }

            public static class Webhooks
            {
                public const string BaseRoute = $"{Storefront.Route}/webhooks";

                public static class Stripe
                {
                    public const string Route = $"{BaseRoute}/stripe";
                    public const string Description = "Receive Stripe webhook events for payment processing";
                    public const string Summary = "Stripe webhook receiver";
                }
            }
        }
    }
}
