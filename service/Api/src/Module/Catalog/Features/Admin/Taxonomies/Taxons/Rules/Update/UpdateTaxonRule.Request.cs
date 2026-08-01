using Module.Catalog.Features.Admin.Taxons.Rules.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Update;

public static partial class UpdateTaxonRule
{
    public record Request : TaxonRuleRequest
    {
        public Guid TaxonId { get; init; }
    }
}