namespace Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Models;

public abstract record VariantImageParameters
{
    public string? Alt { get; init; } = null;
    public int Position { get; init; } = 0;
    public string Type { get; init; } = string.Empty;
}