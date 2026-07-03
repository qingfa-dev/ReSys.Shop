using Module.Catalog.Domain.Products.Variants.Prices;

namespace Module.UnitTests.Catalog.Domain.Products.Variants.Prices;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Price")]
public class PriceMethodTests
{
    [Fact(DisplayName = "Create: Should return Price with correct properties")]
    public void Create_WithValidParameters_ShouldReturnPrice()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var amount = 19.99m;
        var currency = "USD";

        // Act
        var result = PriceMethod.Create(amount, currency, variantId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.VariantId.Should().Be(variantId);
        result.Value.Amount.Should().Be(amount);
        result.Value.Currency.Should().Be(currency);
    }

    [Fact(DisplayName = "Update: Should update amount and currency")]
    public void Update_WithValidParameters_ShouldUpdateProperties()
    {
        var priceResult = PriceMethod.Create(10m, "USD");
        priceResult.IsSuccess.Should().BeTrue();
        var price = priceResult.Value;
        var newAmount = 15.50m;
        var newCurrency = "EUR";
        var compareAt = 20m;
        var countryIso = "US";

        var result = price.Update(newAmount, newCurrency, compareAt, countryIso);

        result.IsSuccess.Should().BeTrue();
        price.Amount.Should().Be(newAmount);
        price.Currency.Should().Be(newCurrency);
        price.CompareAtAmount.Should().Be(compareAt);
        price.CountryIso.Should().Be(countryIso);
    }

    [Fact(DisplayName = "Update: Partial update with only amount should preserve others")]
    public void Update_WithOnlyAmount_ShouldPreserveOthers()
    {
        var priceResult = PriceMethod.Create(10m, "USD", compareAtAmount: 20m, countryIso: "US");
        priceResult.IsSuccess.Should().BeTrue();
        var price = priceResult.Value;

        var result = price.Update(amount: 15m);

        result.IsSuccess.Should().BeTrue();
        price.Amount.Should().Be(15m);
        price.Currency.Should().Be("USD");
        price.CompareAtAmount.Should().Be(20m);
        price.CountryIso.Should().Be("US");
    }

    [Fact(DisplayName = "Delete: Should mark as deleted and set DeletedAt")]
    public void Delete_WhenCalled_ShouldSetDeletedAt()
    {
        var priceResult = PriceMethod.Create(10m, "USD");
        priceResult.IsSuccess.Should().BeTrue();
        var price = priceResult.Value;

        var result = price.Delete();

        result.IsSuccess.Should().BeTrue();
        price.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Delete: When already deleted should return error")]
    public void Delete_WhenAlreadyDeleted_ShouldReturnError()
    {
        var priceResult = PriceMethod.Create(10m, "USD");
        priceResult.IsSuccess.Should().BeTrue();
        var price = priceResult.Value;
        price.Delete();

        var result = price.Delete();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(PriceResult.Errors.AlreadyDeleted.Code);
    }
}
