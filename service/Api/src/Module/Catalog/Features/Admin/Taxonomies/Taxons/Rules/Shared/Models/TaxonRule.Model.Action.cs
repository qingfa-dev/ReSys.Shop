namespace Module.Catalog.Features.Admin.Taxons.Rules.Shared.Models;

public abstract record TaxonRuleActionParameters
{
    public Guid TaxonId { get; init; }
    public Guid RuleId { get; init; }
}
