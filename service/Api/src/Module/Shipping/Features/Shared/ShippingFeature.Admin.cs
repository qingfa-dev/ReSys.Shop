using Shared.Security.Identity.Domain.Permissions;

namespace Module.Shipping.Features.Shared;

public static partial class ShippingFeature
{
    public static class Admin
    {
        public const string Route = "api/shipping";

        public static class ShippingMethods
        {
            public const string BaseRoute = $"{Route}/shipping-methods";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all shipping methods with paging, sorting, and filtering";
                public const string Summary = "Get all shipping methods";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Methods.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a shipping method by identifier";
                public const string Summary = "Get shipping method by ID";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Methods.Read;
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new shipping method";
                public const string Summary = "Create shipping method";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Methods.Create;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update a shipping method";
                public const string Summary = "Update shipping method";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Methods.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Soft-delete a shipping method";
                public const string Summary = "Delete shipping method";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Methods.Delete;
            }

            public static class Activate
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/activate";
                public const string Description = "Activate a shipping method";
                public const string Summary = "Activate shipping method";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Methods.Activate;
            }

            public static class Deactivate
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/deactivate";
                public const string Description = "Deactivate a shipping method";
                public const string Summary = "Deactivate shipping method";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Methods.Deactivate;
            }
        }

        public static class ShippingRates
        {
            public const string BaseRoute = $"{Route}/shipping-rates";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all shipping rates with paging, sorting, and filtering";
                public const string Summary = "Get all shipping rates";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Rates.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a shipping rate by identifier";
                public const string Summary = "Get shipping rate by ID";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Rates.Read;
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new shipping rate";
                public const string Summary = "Create shipping rate";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Rates.Create;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update a shipping rate";
                public const string Summary = "Update shipping rate";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Rates.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Soft-delete a shipping rate";
                public const string Summary = "Delete shipping rate";
                public static PermissionMetadata Permission => ShippingFeatureMetadata.Rates.Delete;
            }
        }
    }
}
