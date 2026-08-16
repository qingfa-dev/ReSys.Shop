using Module.Catalog.Domain.Variants.Prices;
using Module.Catalog.Features.Admin.Shared.Mappings;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Prices.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Price")]
[Trait("Concern", "Mapping")]
public class PriceMappingTests
{
    [Fact(DisplayName = "MapToDomain: Should map PriceRequest to new Price entity")]
    public void MapToDomain_Create_ShouldMapRequestToEntity()
    {
        var variantId = Guid.NewGuid();
        var request = new PriceRequest { Amount = 19.99m, Currency = "USD", CompareAtAmount = 29.99m, CountryIso = "US" };

        var result = request.MapToDomain(variantId);
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.VariantId.Should().Be(variantId);
        entity.Amount.Should().Be(request.Amount);
        entity.Currency.Should().Be(request.Currency);
        entity.CompareAtAmount.Should().Be(request.CompareAtAmount);
        entity.CountryIso.Should().Be(request.CountryIso);
    }

    [Fact(DisplayName = "MapToDomain (Update): Should update existing Price entity")]
    public void MapToDomain_Update_ShouldUpdateEntity()
    {
        var request = new PriceRequest { Amount = 25.00m, Currency = "EUR", CompareAtAmount = 35.00m, CountryIso = "GB" };

        var entity = PriceMethod.Create(10m, "USD").Value;

        var result = request.MapToDomain(entity);

        result.IsSuccess.Should().BeTrue();
        entity.Amount.Should().Be(25.00m);
        entity.Currency.Should().Be("EUR");
        entity.CompareAtAmount.Should().Be(35.00m);
        entity.CountryIso.Should().Be("GB");
    }

    [Fact(DisplayName = "MapToResponse: Should map Price entity to PriceResponse")]
    public void MapToResponse_ShouldMapEntityToResponse()
    {
        var variantId = Guid.NewGuid();
        var entity = PriceMethod.Create(19.99m, "USD", variantId: variantId, compareAtAmount: 29.99m, countryIso: "US").Value;

        var result = entity.MapToDetail<PriceResponse>();

        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.VariantId.Should().Be(variantId);
        result.Amount.Should().Be(19.99m);
        result.Currency.Should().Be("USD");
        result.CompareAtAmount.Should().Be(29.99m);
        result.CountryIso.Should().Be("US");
    }
}
