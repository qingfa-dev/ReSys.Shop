using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Sync;

public static partial class SyncTaxonRules
{
    public sealed record Request : TaxonRuleCollectionParameters;
}
