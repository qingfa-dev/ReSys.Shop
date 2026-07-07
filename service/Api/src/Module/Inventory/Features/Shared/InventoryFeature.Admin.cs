using Shared.Security.Identity.Domain.Permissions;

namespace Module.Inventory.Features.Shared;

public static partial class InventoryFeature
{
    public static class Admin
    {
        public const string Route = "api/inventory";

        public static class StockLocations
        {
            public const string BaseRoute = $"{Route}/stock-locations";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new stock location";
                public const string Summary = "Create stock location";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockLocation.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all stock locations";
                public const string Summary = "Get all stock locations";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockLocation.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a stock location by identifier";
                public const string Summary = "Get stock location by ID";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockLocation.Read;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update an existing stock location";
                public const string Summary = "Update stock location";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockLocation.Update;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete a stock location";
                public const string Summary = "Delete stock location";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockLocation.Delete;
            }

            public static class SetDefault
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/default";
                public const string Description = "Set a stock location as the default";
                public const string Summary = "Set default stock location";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockLocation.Update;
            }
        }

        public static class StockItems
        {
            public const string BaseRoute = $"{Route}/stock-items";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new stock item";
                public const string Summary = "Create stock item";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockItem.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all stock items";
                public const string Summary = "Get all stock items";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockItem.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a stock item by identifier";
                public const string Summary = "Get stock item by ID";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockItem.Read;
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update an existing stock item";
                public const string Summary = "Update stock item";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockItem.Update;
            }

            public static class BulkAdjust
            {
                public const string Route = $"{BaseRoute}/bulk-adjust";
                public const string Description = "Bulk adjust stock item quantities";
                public const string Summary = "Bulk adjust stock items";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockItem.Adjust;
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete a stock item";
                public const string Summary = "Delete stock item";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockItem.Delete;
            }

            public static class Restock
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/restock";
                public const string Description = "Restock a stock item and fulfill backorders";
                public const string Summary = "Restock stock item";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockItem.Adjust;
            }

            public static class LowStock
            {
                public const string Route = $"{BaseRoute}/low-stock";
                public const string Description = "Retrieve stock items below their location's low-stock threshold";
                public const string Summary = "Get low stock items";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockItem.List;
            }

            public static class StockSummary
            {
                public const string Route = $"{BaseRoute}/summary";
                public const string Description = "Get consolidated per-variant stock summary across all locations";
                public const string Summary = "Get stock summary";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockItem.List;
            }

            public static class Import
            {
                public const string Route = $"{BaseRoute}/import";
                public const string Description = "Bulk import stock items from CSV";
                public const string Summary = "Import stock items";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockItem.Create;
            }
        }

        public static class StockReservations
        {
            public const string BaseRoute = $"{Route}/stock-reservations";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all stock reservations";
                public const string Summary = "Get all stock reservations";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockReservations.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a stock reservation by identifier";
                public const string Summary = "Get stock reservation by ID";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockReservations.Read;
            }

            public static class Cancel
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/cancel";
                public const string Description = "Admin cancel a stock reservation";
                public const string Summary = "Cancel stock reservation";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockReservations.Cancel;
            }
        }

        public static class StockTransfers
        {
            public const string BaseRoute = $"{Route}/stock-transfers";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new stock transfer between locations";
                public const string Summary = "Create stock transfer";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockTransfers.Create;
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all stock transfers";
                public const string Summary = "Get all stock transfers";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockTransfers.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a stock transfer by identifier";
                public const string Summary = "Get stock transfer by ID";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockTransfers.Read;
            }

            public static class Transfer
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/transfer";
                public const string Description = "Execute a stock transfer (Draft → InTransit)";
                public const string Summary = "Execute stock transfer";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockTransfers.Update;
            }

            public static class Receive
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/receive";
                public const string Description = "Receive items at destination (InTransit → Received)";
                public const string Summary = "Receive stock transfer";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockTransfers.Update;
            }

            public static class Cancel
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/cancel";
                public const string Description = "Cancel a stock transfer (Draft|InTransit → Canceled)";
                public const string Summary = "Cancel stock transfer";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockTransfers.Cancel;
            }
        }

        public static class StockMovements
        {
            public const string BaseRoute = $"{Route}/stock-movements";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all stock movements";
                public const string Summary = "Get all stock movements";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockMovements.List;
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a stock movement by identifier";
                public const string Summary = "Get stock movement by ID";
                public static PermissionMetadata Permission => InventoryFeatureMetadata.StockMovements.Read;
            }
        }
    }
}
