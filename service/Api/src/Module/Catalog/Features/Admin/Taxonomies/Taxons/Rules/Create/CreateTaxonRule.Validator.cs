using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Validations;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Create;

public static partial class CreateTaxonRule
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(m => m.TaxonomyId)
                .ApplyTaxonomyIdRules();
            RuleFor(m => m.TaxonId)
                .ApplyTaxonIdRules();
            RuleFor(m => m.Request)
                .ApplyTaxonRuleParameterRules();
        }
    }
}
