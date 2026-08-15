namespace Module.Catalog.Features.Admin.Dashboard.Get.Shared.Models;

public abstract record CatalogDashboardParameters
{
    public int TotalProducts { get; init; }
    public int ActiveProducts { get; init; }
    public int DraftProducts { get; init; }
    public int TotalVariants { get; init; }
    public int TotalTaxonomies { get; init; }
    public int TotalTaxons { get; init; }
    public List<RecentProductData> RecentProducts { get; init; } = [];
}

public sealed record RecentProductData
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string Slug { get; init; } = default!;
    public DateTimeOffset CreatedAtUtc { get; init; }
}
