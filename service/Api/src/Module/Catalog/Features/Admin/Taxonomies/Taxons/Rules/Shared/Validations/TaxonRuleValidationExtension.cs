using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxons.Rules.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Shared.Validations;

public static partial class TaxonRuleValidations
{
    public sealed class TaxonRuleParameterValidator : AbstractValidator<TaxonRuleParameter>
    {
        public TaxonRuleParameterValidator()
        {
            RuleFor(x => x.Type).ApplyTypeRules();
            RuleFor(x => x.MatchPolicy).ApplyMatchPolicyRules();
            RuleFor(x => x.Value).ApplyValueRules();
        }
    }

    public static IRuleBuilderOptions<T, TaxonRuleParameter> ApplyTaxonRuleParameterRules<T>(
        this IRuleBuilder<T, TaxonRuleParameter> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new TaxonRuleParameterValidator());
    }
}