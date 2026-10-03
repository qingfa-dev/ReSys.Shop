namespace Module.Catalog.Features.Storefront.Products.Shared;

/// <summary>
/// Storefront-specific constants for product querying.
/// Provides simple aliases (e.g. "Price") that the frontend can use
/// instead of EF Core navigation paths (e.g. "Variants.Prices.Amount").
/// </summary>
public static class StoreProductConstant
{
    /// <summary>
    /// Mapping from simple storefront sort aliases to actual EF Core property paths.
    /// </summary>
    public static IReadOnlyDictionary<string, string> SortAliasMap { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Price"] = "Variants.Prices.Amount",
            ["Name"] = "Name",
            ["CreatedAtUtc"] = "CreatedAtUtc",
            ["ModifiedAtUtc"] = "ModifiedAtUtc",
            ["AvailableOn"] = "AvailableOn",
        };

    /// <summary>
    /// Allowed sort fields for storefront product listing.
    /// Includes both simple aliases and full navigation paths.
    /// </summary>
    public static IReadOnlySet<string> AllowedSortFields { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Name",
            "CreatedAtUtc",
            "ModifiedAtUtc",
            "AvailableOn",
            "Price",
            "Variants.Prices.Amount",
        };

    /// <summary>
    /// Resolve a sort alias to its actual EF Core property path.
    /// Returns the original field if no alias mapping exists.
    /// </summary>
    public static string ResolveSortField(string field)
    {
        return SortAliasMap.TryGetValue(field, out var resolved) ? resolved : field;
    }
}
