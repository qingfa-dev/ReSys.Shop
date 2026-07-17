using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Inventory.Features.Shared;

public static class InventoryDashboardFeature
{
    public const string Route = "api/inventory/dashboard";

    public static class Tags
    {
        public static readonly string[] Inventory = ["Inventory"];
    }

    public static class Admin
    {
        public static class Get
        {
            public const string Route = InventoryDashboardFeature.Route;
            public const string Description = "Get inventory dashboard metrics including stock levels, locations, and recent movements";
            public const string Summary = "Get inventory dashboard";
            public static PermissionMetadata Permission => DashboardFeatureMetadata.Inventory.List;
        }
    }
}
