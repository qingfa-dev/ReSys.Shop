using Module.Catalog.Features.Admin.Taxons.Rules.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Delete;

public static partial class DeleteTaxonRule
{
    public sealed record Request : TaxonRuleActionParameters;
}
