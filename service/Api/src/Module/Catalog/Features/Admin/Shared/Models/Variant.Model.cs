using Module.Catalog.Domain.Variants;

namespace Module.Catalog.Features.Admin.Shared.Models;

public abstract record VariantParameters
{
    public string Sku { get; init; } = string.Empty;
    public int Position { get; init; }
    public bool TrackInventory { get; init; } = true;
    public decimal? Weight { get; init; }
    public WeightUnit? WeightUnit { get; init; }
    public decimal? Height { get; init; }
    public decimal? Width { get; init; }
    public decimal? Depth { get; init; }
    public DimensionUnit? DimensionsUnit { get; init; }
    public decimal? Price { get; init; }
    public decimal? CostPrice { get; init; }
    public string? CostCurrency { get; init; }
    public Guid ProductId { get; init; }
}

public record VariantRequest : VariantParameters
{
    public bool IsMaster { get; init; }
    public List<Guid>? OptionValueIds { get; init; }
}

public record VariantListItemResponse : VariantParameters
{
    public Guid Id { get; init; }
    public bool IsMaster { get; init; }
}

public record VariantDetailResponse : VariantListItemResponse
{
    public DateTimeOffset? DiscontinuedOn { get; init; }
    public int PricesCount { get; init; }
}
