using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Sync;

public static partial class SyncTaxonRules
{
    // EXCEPTION: feature-specific collection wrapper — inner item inherits from TaxonRuleListResponse
    public record Response
    {
        public List<TaxonRuleItem> Rules { get; init; } = [];
    }

    public record TaxonRuleItem : TaxonRuleListResponse;
}