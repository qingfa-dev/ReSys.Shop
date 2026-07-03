namespace Module.Catalog.Features.Admin.Taxonomies.Shared.Models;


public abstract record TaxonomyParameters(string Name = "", string? Presentation = "", int Position = 0);