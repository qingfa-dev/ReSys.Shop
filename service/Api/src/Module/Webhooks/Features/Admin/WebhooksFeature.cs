using Shared.Security.Identity.Domain.Permissions;

namespace Module.Webhooks.Features.Admin;

public static class WebhooksFeature
{
    public const string Module = "Webhooks";

    public static class Tags
    {
        public const string Subscription = "Webhooks-Subscriptions";
    }

    public static class Admin
    {
        public static class Subscriptions
        {
            public static class Create
            {
                public const string Route = "api/webhooks/subscriptions";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "Create");
                public const string Summary = "Create a webhook subscription";
                public const string Description = "Creates a new webhook subscription for receiving events.";
            }

            public static class GetById
            {
                public const string Route = "api/webhooks/subscriptions/{id}";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "GetById");
                public const string Summary = "Get a webhook subscription by ID";
                public const string Description = "Retrieves a specific webhook subscription.";
            }

            public static class GetPaged
            {
                public const string Route = "api/webhooks/subscriptions";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "GetPaged");
                public const string Summary = "List webhook subscriptions";
                public const string Description = "Retrieves a paged list of webhook subscriptions.";
            }

            public static class Update
            {
                public const string Route = "api/webhooks/subscriptions/{id}";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "Update");
                public const string Summary = "Update a webhook subscription";
                public const string Description = "Updates an existing webhook subscription.";
            }

            public static class Delete
            {
                public const string Route = "api/webhooks/subscriptions/{id}";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "Delete");
                public const string Summary = "Delete a webhook subscription";
                public const string Description = "Deletes an existing webhook subscription.";
            }

            public static class Test
            {
                public const string Route = "api/webhooks/subscriptions/{id}/test";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "Test");
                public const string Summary = "Test a webhook subscription";
                public const string Description = "Sends a test event to the webhook subscription URL.";
            }
        }
    }
}
