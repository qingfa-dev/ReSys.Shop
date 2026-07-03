using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Shared.Mappings;

/// <summary>
/// Maps between VariantRequest DTOs and Variant domain entities.
/// </summary>
public static partial class VariantMapping
{
    /// <summary>
    /// Maps a variant request to a new Variant domain entity via factory Create.
    /// </summary>
    /// <typeparam name="T">The request type deriving from <see cref="VariantRequest"/>.</typeparam>
    /// <param name="request">The request payload with variant attributes.</param>
    /// <param name="productId">The parent product ID.</param>
    /// <returns>A result containing the new Variant entity or validation failures.</returns>
    public static Result<Variant> MapToDomain<T>(this T request, Guid productId) where T : VariantRequest
    {
        return VariantMethod.Create(
            productId: productId,
            sku: request.Sku,
            isMaster: request.IsMaster,
            position: request.Position);
    }

    public static Result MapToDomain<T>(this T request, Variant variant) where T : VariantRequest
    {
        var result = variant.Update(
            sku: request.Sku,
            position: request.Position,
            trackInventory: request.TrackInventory);
        if (result.IsFailure)
            return result.Errors;

        result = variant.UpdatePricing(
            price: request.Price,
            costPrice: request.CostPrice,
            costCurrency: request.CostCurrency);
        if (result.IsFailure)
            return result.Errors;

        WeightUnit? parsedWeightUnit = request.WeightUnit is not null && Enum.TryParse<WeightUnit>(request.WeightUnit, ignoreCase: true, out var wu) ? wu : null;
        DimensionUnit? parsedDimUnit = request.DimensionsUnit is not null && Enum.TryParse<DimensionUnit>(request.DimensionsUnit, ignoreCase: true, out var du) ? du : null;

        result = variant.UpdatePhysicalSpecs(
            weight: request.Weight,
            weightUnit: parsedWeightUnit,
            height: request.Height,
            width: request.Width,
            depth: request.Depth,
            dimensionsUnit: parsedDimUnit);
        if (result.IsFailure)
            return result.Errors;

        return Result.Ok();
    }
}
