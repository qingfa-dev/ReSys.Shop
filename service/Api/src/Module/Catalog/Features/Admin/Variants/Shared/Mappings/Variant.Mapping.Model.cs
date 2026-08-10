using Module.Catalog.Domain.Variants;
using Module.Catalog.Features.Admin.Variants.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Shared.Mappings;

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
            WeightUnit = entity.WeightUnit?.ToString(),
            Height = entity.Height,
            Width = entity.Width,
            Depth = entity.Depth,
            DimensionsUnit = entity.DimensionsUnit?.ToString(),
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
            WeightUnit = entity.WeightUnit?.ToString(),
            Height = entity.Height,
            Width = entity.Width,
            Depth = entity.Depth,
            DimensionsUnit = entity.DimensionsUnit?.ToString(),
            Price = entity.Price,
            CostPrice = entity.CostPrice,
            CostCurrency = entity.CostCurrency,
        };
    }
}