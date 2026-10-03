using Module.Catalog.Domain.Taxons.Rules;
using Module.Catalog.Features.Admin.Shared.Models;
using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Sync;

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