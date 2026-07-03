using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Validations;

public sealed class ProductOptionTypeAssignmentItemValidator
    : AbstractValidator<ProductOptionTypeAssignmentItem>
{
    public ProductOptionTypeAssignmentItemValidator()
    {
        RuleFor(x => x.OptionTypeId)
            .ApplyOptionTypeIdRules();

        RuleFor(x => x.Position)
            .ApplyPositionRules();
    }
}

public static class ProductOptionTypeAssignmentItemValidatorExtensions
{
    public static IRuleBuilderOptions<T, IEnumerable<ProductOptionTypeAssignmentItem>>
        ApplyProductOptionTypeAssignmentItemRules<T>(
            this IRuleBuilder<T, IEnumerable<ProductOptionTypeAssignmentItem>> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .WithErrorCode(ProductOptionTypeResult.Errors.OptionTypeIdsRequired.Code)
            .WithMessage(ProductOptionTypeResult.Errors.OptionTypeIdsRequired.Message)
            .ForEach(item => item.SetValidator(new ProductOptionTypeAssignmentItemValidator()));
    }
}
