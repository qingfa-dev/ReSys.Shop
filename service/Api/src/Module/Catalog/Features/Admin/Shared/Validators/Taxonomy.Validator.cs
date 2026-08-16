using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Shared.Validators;

public static class TaxonomyValidator
{
    public sealed class TaxonomyParametersValidator : AbstractValidator<TaxonomyParameters>
    {
        public TaxonomyParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Presentation).ApplyPresentationRules();
            RuleFor(x => x.Position).ApplyPositionRules();
        }
    }

    public static IRuleBuilderOptions<T, TaxonomyParameters> ApplyTaxonomyParametersRules<T>(
        this IRuleBuilder<T, TaxonomyParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new TaxonomyParametersValidator());
    }
}
