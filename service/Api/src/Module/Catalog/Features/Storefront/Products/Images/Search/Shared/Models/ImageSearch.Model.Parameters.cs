namespace Module.Catalog.Features.Storefront.Products.Images.Search.Shared.Models;

public abstract record ImageSearchParameters
{
    public required IFormFile Image { get; init; }
    public int TopK { get; init; } = 20;
    public string? Model { get; init; }
}
