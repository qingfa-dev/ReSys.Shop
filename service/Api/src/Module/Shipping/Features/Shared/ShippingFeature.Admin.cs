using BuildingBlocks.Identity.Domain.AccessControls;
using BuildingBlocks.Identity.Domain.AccessControls.Stores;

namespace Module.Shipping.Features.Shared;

public static partial class ShippingFeature
{
    public static class Admin
    {
        public const string Route = "api/shipping";

        public static class ShippingMethods
        {
            public const string BaseRoute = $"{Route}/methods";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all shipping methods";
                public const string Summary = "Get all shipping methods";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.View;
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new shipping method";
                public const string Summary = "Create shipping method";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update a shipping method";
                public const string Summary = "Update shipping method";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a shipping method by identifier";
                public const string Summary = "Get shipping method by ID";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.View;
            }

            public static class Activate
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/activate";
                public const string Description = "Activate a shipping method";
                public const string Summary = "Activate shipping method";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }

            public static class Deactivate
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/deactivate";
                public const string Description = "Deactivate a shipping method";
                public const string Summary = "Deactivate shipping method";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete a shipping method";
                public const string Summary = "Delete shipping method";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }
        }

        public static class MethodRates
        {
            public const string BaseRoute = $"{Route}/method-rates";

            public static class GetAll
            {
                public const string Route = $"{ShippingMethods.BaseRoute}/{{methodId:guid}}/rates";
                public const string Description = "Retrieve all rates for a shipping method";
                public const string Summary = "Get shipping method rates";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.View;
            }

            public static class Create
            {
                public const string Route = $"{ShippingMethods.BaseRoute}/{{methodId:guid}}/rates";
                public const string Description = "Create a rate for a shipping method";
                public const string Summary = "Create shipping method rate";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }

            public static class Update
            {
                public const string Route = $"{ShippingMethods.BaseRoute}/{{methodId:guid}}/rates/{{rateId:guid}}";
                public const string Description = "Update a shipping method rate";
                public const string Summary = "Update shipping method rate";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }

            public static class Delete
            {
                public const string Route = $"{ShippingMethods.BaseRoute}/{{methodId:guid}}/rates/{{rateId:guid}}";
                public const string Description = "Delete a shipping method rate";
                public const string Summary = "Delete shipping method rate";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }
        }

        public static class ShippingRates
        {
            public const string BaseRoute = $"{Route}/rates";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all shipping rates";
                public const string Summary = "Get all shipping rates";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.View;
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a shipping rate";
                public const string Summary = "Create shipping rate";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }
        }

        public static class Shipments
        {
            public const string BaseRoute = $"{Route}/shipments";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all shipments";
                public const string Summary = "Get all shipments";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.View;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a shipment by identifier";
                public const string Summary = "Get shipment by ID";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.View;
            }

            public static class Ship
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/ship";
                public const string Description = "Mark a shipment as shipped";
                public const string Summary = "Mark shipment shipped";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Ship;
            }

            public static class UpdateTracking
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/tracking";
                public const string Description = "Update the tracking number for a shipment";
                public const string Summary = "Update shipment tracking";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new shipment";
                public const string Summary = "Create shipment";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }

            public static class Cancel
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/cancel";
                public const string Description = "Cancel a shipment";
                public const string Summary = "Cancel shipment";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete a shipment";
                public const string Summary = "Delete shipment";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }

            public static class MarkReady
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/ready";
                public const string Description = "Mark a shipment as ready for shipping";
                public const string Summary = "Mark shipment ready";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }

            public static class MarkPending
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/pending";
                public const string Description = "Move a shipment back to pending";
                public const string Summary = "Mark shipment pending";
                public static PermissionMetadata Permission => PermissionStore.Ordering.Fulfillment.Manage;
            }
        }
    }
}
