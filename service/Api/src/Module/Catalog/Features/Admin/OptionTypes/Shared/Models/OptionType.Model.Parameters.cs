namespace Module.Catalog.Features.Admin.OptionTypes.Shared.Models;


public abstract record OptionTypeParameters
{
    public string Name { get; init; } = string.Empty;
    public string Presentation { get; init; } = string.Empty;
    public int Position { get; init; } = 0;
    public bool Filterable { get; init; } = false;
}