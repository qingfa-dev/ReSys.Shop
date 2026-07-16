using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Dashboard.Features.Shared;

public static partial class DashboardFeature
{
    public const string Route = "api/dashboard";

    public static class Tags
    {
        public static readonly string[] Dashboard = ["Dashboard"];
    }

    public static class Admin
    {
        public static class Get
        {
            public const string Route = DashboardFeature.Route;
            public const string Description = "Retrieve aggregated dashboard metrics including sales, inventory, catalog, and recent activity";
            public const string Summary = "Get dashboard data";
            public static PermissionMetadata Permission => DashboardFeatureMetadata.Sales.List;
        }
    }
}
