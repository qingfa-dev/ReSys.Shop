using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Validations;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Sync;

public static partial class SyncTaxonRules
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(m => m.Request.Rules)
                .NotNull()
                .WithErrorCode(TaxonRuleResult.Errors.RulesRequired.Code)
                .WithMessage(TaxonRuleResult.Errors.RulesRequired.Message);

            RuleForEach(m => m.Request.Rules)
                .SetValidator(new SyncItemValidator());
        }

        public class SyncItemValidator : AbstractValidator<SyncItem>
        {
            public SyncItemValidator()
            {
                RuleFor(x => x).ApplyTaxonRuleParameterRules();
            }
        }
    }
}