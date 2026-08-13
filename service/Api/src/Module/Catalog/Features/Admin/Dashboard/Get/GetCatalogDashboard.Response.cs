namespace Module.Catalog.Features.Admin.Dashboard.Get;

public static partial class GetCatalogDashboard
{
    // EXCEPTION: standalone dashboard aggregate response, no shared base type exists.
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

    public sealed record RecentProductData
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string Slug { get; init; } = default!;
        public DateTimeOffset CreatedAtUtc { get; init; }
    }
}
