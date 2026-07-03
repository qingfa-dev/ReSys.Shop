using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Shared.Validation;

/// <summary>
/// Shared validation rules for product input parameters.
/// Used by CreateProduct and UpdateProduct validators.
/// </summary>
public static class ProductValidation
{
    /// <summary>
    /// Validates ProductParameters — name, slug, description, meta fields.
    /// </summary>
    public sealed class ProductParametersValidator : AbstractValidator<ProductParameters>
    {
        public ProductParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Slug).ApplySlugRules();
            RuleFor(x => x.Description).ApplyDescriptionRules();
            RuleFor(x => x.MetaTitle).ApplyMetaTitleRules();
            RuleFor(x => x.MetaDescription).ApplyMetaDescriptionRules();
            RuleFor(x => x.MetaKeywords).ApplyMetaKeywordsRules();
        }
    }

    public static IRuleBuilderOptions<T, ProductParameters> ApplyProductParametersRules<T>(
        this IRuleBuilder<T, ProductParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new ProductParametersValidator());
    }
}
