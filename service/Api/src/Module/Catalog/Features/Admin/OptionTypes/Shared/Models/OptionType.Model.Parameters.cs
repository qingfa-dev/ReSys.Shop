namespace Module.Catalog.Features.Admin.OptionTypes.Shared.Models;


public abstract record OptionTypeParameters(
    string Name = "",
    string Presentation = "",
    int Position = 0,
    bool Filterable = false);