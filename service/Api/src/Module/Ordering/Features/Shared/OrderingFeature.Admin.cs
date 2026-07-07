using BuildingBlocks.Identity.Domain.AccessControls;
using BuildingBlocks.Identity.Domain.AccessControls.Stores;

using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Ordering.Features.Shared;

public static partial class OrderingFeature
{
    public static class Admin
    {
        public const string Route = "api/ordering";

        public static class Orders
        {
            public const string BaseRoute = $"{Route}/orders";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all orders with paging, sorting, and filtering";
                public const string Summary = "Get all orders";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve an order by identifier";
                public const string Summary = "Get order by ID";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Read;
            }

            public static class UpdateStatus
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/status";
                public const string Description = "Update order status";
                public const string Summary = "Update order status";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class AddAdjustment
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/adjustments";
                public const string Description = "Add an adjustment to an order";
                public const string Summary = "Add order adjustment";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class GetAdjustments
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/adjustments";
                public const string Description = "Retrieve all adjustments for an order";
                public const string Summary = "Get order adjustments";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Read;
            }

            public static class GetAdjustmentById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/adjustments/{{adjustmentId:guid}}";
                public const string Description = "Retrieve a single adjustment for an order";
                public const string Summary = "Get order adjustment by ID";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Read;
            }

            public static class UpdateAdjustment
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/adjustments/{{adjustmentId:guid}}";
                public const string Description = "Update an adjustment state or eligibility";
                public const string Summary = "Update order adjustment";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class RemoveAdjustment
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/adjustments/{{adjustmentId:guid}}";
                public const string Description = "Remove an adjustment from an order";
                public const string Summary = "Remove order adjustment";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class GetLineItems
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/line-items";
                public const string Description = "Retrieve all line items for an order";
                public const string Summary = "Get order line items";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Read;
            }

            public static class GetLineItemById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/line-items/{{lineItemId:guid}}";
                public const string Description = "Retrieve a single line item for an order";
                public const string Summary = "Get order line item by ID";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Read;
            }

            public static class AddLineItem
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/line-items";
                public const string Description = "Add a line item to an order";
                public const string Summary = "Add order line item";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class UpdateLineItem
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/line-items/{{lineItemId:guid}}";
                public const string Description = "Update a line item on an order";
                public const string Summary = "Update order line item";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class RemoveLineItem
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/line-items/{{lineItemId:guid}}";
                public const string Description = "Remove a line item from an order";
                public const string Summary = "Remove order line item";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class Cancel
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/cancel";
                public const string Description = "Admin cancel an order";
                public const string Summary = "Cancel order";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class Complete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/complete";
                public const string Description = "Mark an order as completed";
                public const string Summary = "Complete order";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class Approve
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/approve";
                public const string Description = "Approve a placed order";
                public const string Summary = "Approve order";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class Resume
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/resume";
                public const string Description = "Resume a previously canceled order";
                public const string Summary = "Resume order";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class ResendConfirmationEmail
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/resend-confirmation-email";
                public const string Description = "Resend order confirmation email";
                public const string Summary = "Resend confirmation email";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class UpdateDetails
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update order details (email, instructions)";
                public const string Summary = "Update order details";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class UpdateShipAddress
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/ship-address";
                public const string Description = "Update shipping address for an order";
                public const string Summary = "Update order ship address";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class UpdateBillAddress
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/bill-address";
                public const string Description = "Update billing address for an order";
                public const string Summary = "Update order bill address";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class UpdateShippingMethod
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/shipping-method";
                public const string Description = "Update shipping method for an order";
                public const string Summary = "Update order shipping method";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Update;
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Admin create a new order";
                public const string Summary = "Create order";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Create;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Admin delete an order";
                public const string Summary = "Delete order";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Orders.Delete;
            }

            public static class Shipments
            {
                public const string Route = $"{BaseRoute}/{{orderId:guid}}/shipments";
                public const string Description = "Retrieve all shipments for an order";
                public const string Summary = "Get order shipments";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Fulfillment.Read;
            }

            public static class CreateShipment
            {
                public const string Route = $"{BaseRoute}/{{orderId:guid}}/shipments";
                public const string Description = "Create a new shipment for an order";
                public const string Summary = "Create order shipment";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Fulfillment.Manage;
            }

            public static class GetShipmentById
            {
                public const string Route = $"{BaseRoute}/{{orderId:guid}}/shipments/{{shipmentId:guid}}";
                public const string Description = "Retrieve a single shipment for an order";
                public const string Summary = "Get order shipment by ID";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Fulfillment.Read;
            }

            public static class UpdateShipment
            {
                public const string Route = $"{BaseRoute}/{{orderId:guid}}/shipments/{{shipmentId:guid}}";
                public const string Description = "Update a shipment for an order";
                public const string Summary = "Update order shipment";
                public static PermissionMetadata Permission => OrderingFeatureMetadata.Fulfillment.Manage;
            }
        }
    }
}
