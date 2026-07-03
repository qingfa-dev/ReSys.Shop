using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Features.Admin.Products.Variants.Prices.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Prices.Shared.Mappings;

/// <summary>
/// Maps between PriceRequest DTOs and Price domain entities, and Price entities to response DTOs.
/// </summary>
public static partial class PriceMapping
{
    /// <summary>
    /// Maps a price request to a new Price domain entity for the given variant.
    /// </summary>
    /// <param name="request">The price request payload.</param>
    /// <param name="variantId">The owning variant ID.</param>
    /// <returns>A result containing the new Price entity or validation failures.</returns>
    public static Result<Price> MapToDomain<T>(this T request, Guid variantId) where T : PriceRequest
    {
        return PriceMethod.Create(
            amount: request.Amount,
            currency: request.Currency,
            variantId: variantId,
            compareAtAmount: request.CompareAtAmount,
            countryIso: request.CountryIso);
    }

    public static Result MapToDomain<T>(this T request, Price price) where T : PriceRequest
    {
        return price.Update(
            amount: request.Amount,
            currency: request.Currency,
            compareAtAmount: request.CompareAtAmount,
            countryIso: request.CountryIso);
    }

    public static T MapToDetail<T>(this Price entity) where T : PriceResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            VariantId = entity.VariantId ?? Guid.Empty,
            Amount = entity.Amount,
            Currency = entity.Currency,
            CompareAtAmount = entity.CompareAtAmount,
            CountryIso = entity.CountryIso,
        };
    }
}
