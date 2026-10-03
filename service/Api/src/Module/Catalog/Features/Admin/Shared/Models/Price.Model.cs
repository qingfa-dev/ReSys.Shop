namespace Module.Catalog.Features.Admin.Shared.Models;

public abstract record PriceParameters
{
    public decimal? Amount { get; init; } = null;
    public string Currency { get; init; } = string.Empty;
    public decimal? CompareAtAmount { get; init; } = null;
    public string? CountryIso { get; init; } = null;
}

public record PriceRequest : PriceParameters
{
    public Guid VariantId { get; init; }
}

public record PriceResponse : PriceParameters
{
    public Guid Id { get; init; }
    public Guid VariantId { get; init; }
}
