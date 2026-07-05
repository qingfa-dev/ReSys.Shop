using Module.Catalog.Features.Admin.Products.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Update;

public static partial class UpdateProduct
{
public record Request : ProductRequest
    {
        public string? Sku { get; init; }
        public new bool? TrackInventory { get; init; }
        public decimal? Price { get; init; }
        public decimal? CostPrice { get; init; }
        public string? CostCurrency { get; init; }
        public decimal? Weight { get; init; }
        public string? WeightUnit { get; init; }
        public decimal? Height { get; init; }
        public decimal? Width { get; init; }
        public decimal? Depth { get; init; }
        public string? DimensionsUnit { get; init; }
    }
}
