namespace Module.Catalog.Features.Storefront.Shared.Models;

public record VisualSearchModelResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Dimension { get; init; }
    public string? Description { get; init; }
    public bool IsOnnx { get; init; }
}
