namespace Module.Catalog.Features.Admin.OptionTypes.Shared.Models;


public abstract record OptionTypeParameters : INamedParameters, ISortableParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public int Position { get; init; } = 0;
    public bool Filterable { get; init; } = false;
}