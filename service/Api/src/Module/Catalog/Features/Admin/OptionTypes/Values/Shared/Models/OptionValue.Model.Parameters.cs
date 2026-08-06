namespace Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Models;


// public abstract record OptionValueParameters(string Name = "", string Presentation = "", int Position = 0);

public abstract record OptionValueParameters : INamedParameters, ISortableParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public int Position { get; init; } = 0;
}