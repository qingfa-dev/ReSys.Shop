using Module.Catalog.Domain.Products.Variants.Prices;

namespace Module.UnitTests.Catalog.Domain.Products.Variants.Prices;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "PriceHistory")]
public class PriceHistoryExtensionsTests
{
    [Fact(DisplayName = "Create: Should return PriceHistory with correct properties")]
    public void Create_WithValidParameters_ShouldReturnPriceHistory()
    {
        var priceId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var recordedAt = DateTimeOffset.UtcNow.AddDays(-1);

        var result = PriceHistoryExtensions.Create(19.99m, "USD", priceId, variantId, recordedAt);
        var history = result.Value;

        result.IsSuccess.Should().BeTrue();
        history.Amount.Should().Be(19.99m);
        history.Currency.Should().Be("USD");
        history.PriceId.Should().Be(priceId);
        history.VariantId.Should().Be(variantId);
        history.RecordedAt.Should().Be(recordedAt);
    }

    [Fact(DisplayName = "Create: Should use UtcNow for RecordedAt when not provided")]
    public void Create_WhenRecordedAtNull_ShouldUseUtcNow()
    {
        var result = PriceHistoryExtensions.Create(9.99m, "EUR", Guid.NewGuid(), Guid.NewGuid());
        var history = result.Value;

        result.IsSuccess.Should().BeTrue();
        history.RecordedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "DisplayAmount: Should format amount with currency")]
    public void DisplayAmount_ShouldFormatCorrectly()
    {
        var history = PriceHistoryExtensions.Create(19.99m, "USD", Guid.NewGuid(), Guid.NewGuid()).Value;

        var display = history.DisplayAmount();

        display.Should().Be("19.99 USD");
    }

    [Fact(DisplayName = "AmountInCents: Should convert to cents")]
    public void AmountInCents_ShouldReturnCorrectValue()
    {
        var history = PriceHistoryExtensions.Create(19.99m, "USD", Guid.NewGuid(), Guid.NewGuid()).Value;

        var cents = history.AmountInCents();

        cents.Should().Be(1999L);
    }
}
