using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Shared.Validators;

public static class TaxonValidator
{
    public sealed class TaxonParametersValidator : AbstractValidator<TaxonParameters>
    {
        public TaxonParametersValidator()
        {
            #region Relationship
            RuleFor(x => x.TaxonomyId)
                .ApplyTaxonomyIdRules();
            #endregion
            #region Properties
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Presentation).ApplyPresentationRules();
            RuleFor(x => x.Description).ApplyDescriptionRules();
            RuleFor(x => x.Position).ApplyPositionRules();
            #endregion

            #region SEO
            RuleFor(x => x.Slug).ApplySlugRules();
            RuleFor(x => x.MetaTitle).ApplyMetaTitleRules();
            RuleFor(x => x.MetaDescription).ApplyMetaDescriptionRules();
            RuleFor(x => x.MetaKeywords).ApplyMetaKeywordsRules();
            #endregion

            #region Images
            RuleFor(x => x.ImageUrl).ApplyImageUrlRules();
            RuleFor(x => x.SquareImageUrl).ApplySquareImageUrlRules();
            #endregion

            #region Settings
            RuleFor(x => x.RulesMatchPolicy).ApplyRulesMatchPolicyRules();
            RuleFor(x => x.SortOrder).ApplySortOrderRules();
            #endregion

        }
    }

    public static IRuleBuilderOptions<T, TaxonParameters> ApplyTaxonomyParametersRules<T>(
        this IRuleBuilder<T, TaxonParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new TaxonParametersValidator());
    }
}
