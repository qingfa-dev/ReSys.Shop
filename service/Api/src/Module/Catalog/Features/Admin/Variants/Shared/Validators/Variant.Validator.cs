using Module.Catalog.Domain.Variants;
using Module.Catalog.Features.Admin.Variants.Shared.Models;



namespace Module.Catalog.Features.Admin.Variants.Shared.Validators;

/// <summary>
/// Shared validation rules for variant input parameters.
/// Used by AddVariant and UpdateVariant validators.
/// </summary>
public static class VariantValidator
{
    /// <summary>
    /// Validates VariantParameters — SKU, position, pricing, weight, dimensions.
    /// </summary>
    public sealed class VariantParametersValidator : AbstractValidator<VariantParameters>
    {
        public VariantParametersValidator()
        {
            RuleFor(x => x.Sku).ApplySkuRules();
            RuleFor(x => x.Position).ApplyPositionRules();
            RuleFor(x => x.Price).ApplyPriceRules();
            RuleFor(x => x.Weight).ApplyWeightRules();
            RuleFor(x => x.WeightUnit)
                .Must(v => string.IsNullOrEmpty(v) || Enum.TryParse<WeightUnit>(v, ignoreCase: true, out _))
                .WithMessage("Weight unit must be one of: G, Kg, Lb, Oz.");
            RuleFor(x => x.Height).ApplyDimensionRules();
            RuleFor(x => x.Width).ApplyDimensionRules();
            RuleFor(x => x.Depth).ApplyDimensionRules();
            RuleFor(x => x.DimensionsUnit)
                .Must(v => string.IsNullOrEmpty(v) || Enum.TryParse<DimensionUnit>(v, ignoreCase: true, out _))
                .WithMessage("Dimensions unit must be one of: Mm, Cm, In, Ft.");
            RuleFor(x => x.CostPrice).ApplyCostPriceRules();
            RuleFor(x => x.CostCurrency).ApplyCostCurrencyRules();
        }
    }

    public static IRuleBuilderOptions<T, VariantParameters> ApplyVariantParametersRules<T>(
        this IRuleBuilder<T, VariantParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new VariantParametersValidator());
    }
}