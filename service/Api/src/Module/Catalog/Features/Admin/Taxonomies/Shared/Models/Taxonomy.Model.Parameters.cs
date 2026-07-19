namespace Module.Catalog.Features.Admin.Taxonomies.Shared.Models;


public abstract record TaxonomyParameters : INamedParameters, ISortableParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; } = string.Empty;
    public int Position { get; init; } = 0;
}