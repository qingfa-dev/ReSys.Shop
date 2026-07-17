namespace Module.Catalog.Features.Admin.Dashboard.Get;

public static partial class GetCatalogDashboard
{
    public sealed record Response
    {
        public int TotalProducts { get; init; }
        public int ActiveProducts { get; init; }
        public int DraftProducts { get; init; }
        public int TotalVariants { get; init; }
        public int TotalTaxonomies { get; init; }
        public int TotalTaxons { get; init; }
        public List<RecentProductData> RecentProducts { get; init; } = [];
    }

    public sealed record RecentProductData(Guid Id, string Name, string Slug, DateTime CreatedAtUtc);
}
