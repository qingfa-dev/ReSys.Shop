namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Shared.Models;

public abstract record PriceParameters
{
    public decimal? Amount { get; init; } = null;
    public string Currency { get; init; } = string.Empty;
    public decimal? CompareAtAmount { get; init; } = null;
    public string? CountryIso { get; init; } = null;
}