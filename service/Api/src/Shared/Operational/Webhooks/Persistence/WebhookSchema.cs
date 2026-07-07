using Shared.Governance.Conventions;

namespace Shared.Operational.Webhooks.Persistence;

public static class WebhookSchema
{
    public static string Name => "webhooks";
    public static class TableNames
    {
        public static string Subscriptions => "WebhookSubscriptions".ToSnakeCase()!;
        public static string Deliveries => "WebhookDeliveries".ToSnakeCase()!;
    }
}
