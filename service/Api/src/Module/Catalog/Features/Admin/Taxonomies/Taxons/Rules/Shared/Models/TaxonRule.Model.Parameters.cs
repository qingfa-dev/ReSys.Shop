namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Models;

public abstract record TaxonRuleParameter
{
    public string Type { get; init; } = string.Empty;
    public string MatchPolicy { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}