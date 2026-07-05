using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Persistence.Constants;

public static class InventorySchema
{
    public const string Name = "inventory";

    public static class TableNames
    {
        public static string StockLocations => nameof(StockLocation).ToSnakeCase()!;
        public static string StockItems => nameof(StockItem).ToSnakeCase()!;
        public static string StockMovements => nameof(StockMovement).ToSnakeCase()!;
        public static string StockReservations => nameof(StockReservation).ToSnakeCase()!;
        public static string StockTransfers => nameof(StockTransfer).ToSnakeCase()!;
        public static string TransferItems => nameof(TransferItem).ToSnakeCase()!;
    }
}
