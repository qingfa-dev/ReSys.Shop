using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Sync;

public static partial class SyncTaxonRules
{
    public sealed record SyncItem : TaxonRuleRequest
    {
        public Guid? Id { get; init; }
    }

    public sealed record Request
    {
        public List<SyncItem> Rules { get; init; } = [];
    }
}