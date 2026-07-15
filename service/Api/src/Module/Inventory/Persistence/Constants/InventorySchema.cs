namespace Module.Inventory.Persistence.Constants;

public static class InventorySchema
{
    public const string Name = "inventory";

    public static class TableNames
    {
        public const string StockLocations = "stock_locations";
        public const string StockItems = "stock_items";
        public const string StockMovements = "stock_movements";
        public const string StockReservations = "stock_reservations";
        public const string StockTransfers = "stock_transfers";
        public const string TransferItems = "transfer_items";
    }
}
