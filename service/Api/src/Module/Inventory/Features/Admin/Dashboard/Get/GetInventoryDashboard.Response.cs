namespace Module.Inventory.Features.Admin.Dashboard.Get;

public static partial class GetInventoryDashboard
{
    public sealed record Response
    {
        public int TotalSkusTracked { get; init; }
        public int InStockCount { get; init; }
        public int OutOfStockCount { get; init; }
        public int LowStockCount { get; init; }
        public int StockLocationCount { get; init; }
        public int ItemsPerLocationAverage { get; init; }
        public List<RecentMovementData> RecentMovements { get; init; } = [];
    }

    public sealed record RecentMovementData
    {
        public Guid Id { get; init; }
        public int Quantity { get; init; }
        public string? Action { get; init; }
        public string? Reason { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }
}
