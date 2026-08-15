using Module.Catalog.Domain.Variants.Prices;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StoreVariantPriceMapping")]
public class StoreVariantPriceMappingTests
{
    [Fact(DisplayName = "MapToStoreItem: Should map Price to StoreVariantPriceListItemRepsonse")]
    public void MapToStoreItem_ShouldMapPriceToListItem()
    {
        var variantId = Guid.NewGuid();
        var price = PriceMethod.Create(
            19.99m, "USD",
            variantId: variantId,
            compareAtAmount: 29.99m,
            countryIso: "US").Value;

        var response = price.MapToStoreItem<StoreVariantPriceListItemRepsonse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(price.Id);
        response.VariantId.Should().Be(variantId);
        response.Amount.Should().Be(19.99m);
        response.Currency.Should().Be("USD");
        response.CompareAtAmount.Should().Be(29.99m);
        response.CountryIso.Should().Be("US");
    }
}
