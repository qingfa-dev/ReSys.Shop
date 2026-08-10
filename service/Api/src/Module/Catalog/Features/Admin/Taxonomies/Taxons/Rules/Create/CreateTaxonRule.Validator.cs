using Module.Catalog.Domain.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxons.Rules.Shared.Validations;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Create;

public static partial class CreateTaxonRule
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(m => m.TaxonId)
                .ApplyTaxonIdRules();
            RuleFor(m => m.Request)
                .ApplyTaxonRuleParameterRules();
        }
    }
}