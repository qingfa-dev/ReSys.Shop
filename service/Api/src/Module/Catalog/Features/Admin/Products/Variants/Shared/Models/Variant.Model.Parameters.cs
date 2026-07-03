namespace Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

public abstract record VariantParameters
{
    public string Sku { get; init; } = string.Empty;
    public int Position { get; init; }
    public bool TrackInventory { get; init; } = true;
    public decimal? Weight { get; init; }
    public string? WeightUnit { get; init; }
    public decimal? Height { get; init; }
    public decimal? Width { get; init; }
    public decimal? Depth { get; init; }
    public string? DimensionsUnit { get; init; }
    public decimal? Price { get; init; }
    public decimal? CostPrice { get; init; }
    public string? CostCurrency { get; init; }
}
