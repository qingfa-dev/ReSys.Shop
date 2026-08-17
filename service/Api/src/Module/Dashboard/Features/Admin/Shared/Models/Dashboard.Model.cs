namespace Module.Dashboard.Features.Admin.Shared.Models;

public abstract record DashboardParameters
{
    public SalesSummaryData Sales { get; init; } = new();
    public InventorySummaryData Inventory { get; init; } = new();
    public CatalogSummaryData Catalog { get; init; } = new();
    public List<ActivityItemData> RecentActivities { get; init; } = [];
}

public sealed record SalesSummaryData
{
    public decimal TotalRevenue { get; init; }
    public int OrderCount { get; init; }
    public decimal AverageOrderValue { get; init; }
    public decimal RevenueTrendPercentage { get; init; }
    public List<TrendPoint> TrendHistory { get; init; } = [];
}

public sealed record TrendPoint
{
    public DateOnly Date { get; init; }
    public decimal Revenue { get; init; }
}

public sealed record InventorySummaryData
{
    public int TotalVariants { get; init; }
    public int OutOfStockCount { get; init; }
    public int LowStockCount { get; init; }
    public decimal StockAccuracyPercentage { get; init; }
}

public sealed record CatalogSummaryData
{
    public int TotalProducts { get; init; }
    public int ActiveProducts { get; init; }
    public int TotalVariants { get; init; }
    public int TotalTaxonomies { get; init; }
    public int TotalTaxons { get; init; }
    public List<RecentProductData> RecentlyAdded { get; init; } = [];
}

public sealed record RecentProductData
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string Slug { get; init; } = default!;
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public sealed record ActivityItemData
{
    public Guid Id { get; init; }
    public ActivityType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ActivityStatus Status { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
