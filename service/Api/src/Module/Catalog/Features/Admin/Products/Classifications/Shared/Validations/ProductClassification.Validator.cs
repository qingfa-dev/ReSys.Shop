using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Features.Admin.Products.Classifications.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Validations;

public sealed class ProductClassificationAssignmentItemValidator
    : AbstractValidator<ProductClassificationAssignmentItem>
{
    public ProductClassificationAssignmentItemValidator()
    {
        RuleFor(x => x.TaxonId)
            .ApplyTaxonIdRules();

        RuleFor(x => x.Position)
            .ApplyPositionRules();
    }
}

public static class ProductClassificationAssignmentItemValidatorExtensions
{
    public static IRuleBuilderOptions<T, IEnumerable<ProductClassificationAssignmentItem>>
        ApplyProductClassificationAssignmentItemRules<T>(
            this IRuleBuilder<T, IEnumerable<ProductClassificationAssignmentItem>> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .WithErrorCode(ClassificationResult.Errors.TaxonIdsRequired.Code)
            .WithMessage(ClassificationResult.Errors.TaxonIdsRequired.Message)
            .ForEach(item => item.SetValidator(new ProductClassificationAssignmentItemValidator()));
    }
}