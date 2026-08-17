using Module.Catalog.Domain.Variants;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Shared.Mappings;

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

        WeightUnit? parsedWeightUnit = request.WeightUnit;
        DimensionUnit? parsedDimUnit = request.DimensionsUnit;

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

/// <summary>
/// Maps between Variant domain entities and response DTOs.
/// </summary>
public static partial class VariantMapping
{
    /// <summary>
    /// Maps a Variant entity to a detail response DTO with all variant attributes including prices count.
    /// </summary>
    /// <typeparam name="T">The response type deriving from <see cref="VariantDetailResponse"/>.</typeparam>
    /// <param name="entity">The variant domain entity.</param>
    /// <returns>A detail response DTO populated from the entity.</returns>
    public static T MapToDetail<T>(this Variant entity) where T : VariantDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            IsMaster = entity.IsMaster,
            Sku = entity.Sku ?? string.Empty,
            Position = entity.Position,
            TrackInventory = entity.TrackInventory,
            Weight = entity.Weight,
            WeightUnit = entity.WeightUnit,
            Height = entity.Height,
            Width = entity.Width,
            Depth = entity.Depth,
            DimensionsUnit = entity.DimensionsUnit,
            Price = entity.Price,
            CostPrice = entity.CostPrice,
            CostCurrency = entity.CostCurrency,
            DiscontinuedOn = entity.DiscontinuedOn,
            PricesCount = entity.Prices.Count,
        };
    }

    public static T MapToListItem<T>(this Variant entity) where T : VariantListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            IsMaster = entity.IsMaster,
            Sku = entity.Sku ?? string.Empty,
            Position = entity.Position,
            TrackInventory = entity.TrackInventory,
            Weight = entity.Weight,
            WeightUnit = entity.WeightUnit,
            Height = entity.Height,
            Width = entity.Width,
            Depth = entity.Depth,
            DimensionsUnit = entity.DimensionsUnit,
            Price = entity.Price,
            CostPrice = entity.CostPrice,
            CostCurrency = entity.CostCurrency,
        };
    }
}
