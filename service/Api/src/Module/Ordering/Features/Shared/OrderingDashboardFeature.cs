using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Ordering.Features.Shared;

public static class OrderingDashboardFeature
{
    public const string Route = "api/ordering/dashboard";

    public static class Tags
    {
        public static readonly string[] Ordering = ["Ordering"];
    }

    public static class Admin
    {
        public static class Get
        {
            public const string Route = OrderingDashboardFeature.Route;
            public const string Description = "Get ordering dashboard metrics including order counts, revenue, and status breakdown";
            public const string Summary = "Get ordering dashboard";
            public static PermissionMetadata Permission => DashboardFeatureMetadata.Orders.List;
        }
    }
}
