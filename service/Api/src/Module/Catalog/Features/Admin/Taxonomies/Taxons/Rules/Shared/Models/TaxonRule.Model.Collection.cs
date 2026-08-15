namespace Module.Catalog.Features.Admin.Taxons.Rules.Shared.Models;

public abstract record TaxonRuleCollectionParameters
{
    public Guid TaxonId { get; init; }
    public List<SyncItem> Rules { get; init; } = [];
}

public sealed record SyncItem : TaxonRuleRequest
{
    public Guid? Id { get; init; }
}
