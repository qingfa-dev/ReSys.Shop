namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Models;


// public abstract record OptionValueParameters(string Name = "", string Presentation = "", int Position = 0);

public abstract record OptionValueParameters
{
    public string Name { get; init; } = string.Empty;
    public string Presentation { get; init; } = string.Empty;
    public int Position { get; init; } = 0;
}