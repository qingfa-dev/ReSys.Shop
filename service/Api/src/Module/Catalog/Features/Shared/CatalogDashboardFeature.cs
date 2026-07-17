using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Catalog.Features.Shared;

public static class CatalogDashboardFeature
{
    public const string Route = "api/catalog/dashboard";

    public static class Tags
    {
        public static readonly string[] Catalog = ["Catalog"];
    }

    public static class Admin
    {
        public static class Get
        {
            public const string Route = CatalogDashboardFeature.Route;
            public const string Description = "Get catalog dashboard metrics including product, variant, and taxonomy counts";
            public const string Summary = "Get catalog dashboard";
            public static PermissionMetadata Permission => DashboardFeatureMetadata.Catalog.List;
        }
    }
}
