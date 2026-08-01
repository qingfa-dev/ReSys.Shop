using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxons.Rules.Shared.Validations;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Update;

public static partial class UpdateTaxonRule
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(m => m.TaxonId)
                .ApplyTaxonIdRules();
            RuleFor(m => m.RuleId)
                .ApplyRuleIdRules();
            RuleFor(m => m.Request)
                .ApplyTaxonRuleParameterRules();
        }
    }
}