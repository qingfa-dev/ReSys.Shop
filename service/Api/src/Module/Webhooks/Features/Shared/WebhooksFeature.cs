namespace Module.Webhooks.Features.Shared;

public static partial class WebhooksFeature
{
    public static class Tags
    {
        public static readonly string[] Webhook = ["Webhook"];
    }

    public static class Admin
    {
        public const string Route = "api/webhooks";

        public static class Subscriptions
        {
            public const string BaseRoute = $"{Route}/subscriptions";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "List webhook subscriptions";
                public const string Summary = "List webhook subscriptions";
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Get a webhook subscription by ID";
                public const string Summary = "Get webhook subscription";
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new webhook subscription";
                public const string Summary = "Create webhook subscription";
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update a webhook subscription";
                public const string Summary = "Update webhook subscription";
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete a webhook subscription";
                public const string Summary = "Delete webhook subscription";
            }

            public static class Test
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/test";
                public const string Description = "Send a test event to the subscription URL";
                public const string Summary = "Test webhook subscription";
            }
        }
    }
}
