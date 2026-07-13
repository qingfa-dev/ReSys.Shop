namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Shared.Models;

public abstract record PriceParameters(decimal? Amount = null, string Currency = "", decimal? CompareAtAmount = null, string? CountryIso = null);