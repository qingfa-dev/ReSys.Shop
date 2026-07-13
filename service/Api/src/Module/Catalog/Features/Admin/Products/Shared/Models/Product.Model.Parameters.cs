namespace Module.Catalog.Features.Admin.Products.Shared.Models;

public abstract record ProductParameters
{
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
    public DateTimeOffset? AvailableOn { get; init; }
    public DateTimeOffset? DiscontinueOn { get; init; }
    public bool TrackInventory { get; init; }
}