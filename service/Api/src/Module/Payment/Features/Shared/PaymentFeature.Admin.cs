using Shared.Security.Identity.Domain.Permissions;

namespace Module.Payment.Features.Shared;

public sealed partial class PaymentFeature
{
    public static class Admin
    {
        public const string Route = "api/payment";

        public static class Payments
        {
            public const string BaseRoute = $"{Route}/payments";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all payments with paging, sorting, and filtering";
                public const string Summary = "Get all payments";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.Payments.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a payment by identifier";
                public const string Summary = "Get payment by ID";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.Payments.List;
            }

            public static class Capture
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/capture";
                public const string Description = "Capture an authorized payment";
                public const string Summary = "Capture payment";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.Payments.Capture;
            }

            public static class Void
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/void";
                public const string Description = "Void an authorized payment";
                public const string Summary = "Void payment";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.Payments.Void;
            }

            public static class Refund
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/refund";
                public const string Description = "Refund a captured payment";
                public const string Summary = "Refund payment";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.Payments.Refund;
            }
        }

        public static class PaymentMethods
        {
            public const string BaseRoute = $"{Route}/payment-methods";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all payment methods with paging, sorting, and filtering";
                public const string Summary = "Get all payment methods";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.PaymentMethods.Read;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a payment method by identifier";
                public const string Summary = "Get payment method by ID";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.PaymentMethods.Read;
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new payment method";
                public const string Summary = "Create payment method";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.PaymentMethods.Create;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update a payment method";
                public const string Summary = "Update payment method";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.PaymentMethods.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Soft-delete a payment method";
                public const string Summary = "Delete payment method";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.PaymentMethods.Delete;
            }

            public static class Activate
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/activate";
                public const string Description = "Activate a payment method";
                public const string Summary = "Activate payment method";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.PaymentMethods.Activate;
            }

            public static class Deactivate
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/deactivate";
                public const string Description = "Deactivate a payment method";
                public const string Summary = "Deactivate payment method";
                public static PermissionMetadata Permission => PaymentFeatureMetadata.PaymentMethods.Deactivate;
            }
        }
    }
}
