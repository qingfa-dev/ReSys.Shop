using Module.Inventory.Domain.StockReservations;

namespace Module.UnitTests.Inventory.Domain.StockReservations;

public class StockReservationResultTests
{
    [Fact(DisplayName = "StockLocationRequired: returns validation error with correct code")]
    public void StockLocationRequired_HasCorrectCode()
    {
        var error = StockReservationResult.Errors.StockLocationRequired;
        error.Code.Should().Be("StockReservation.Cart.StockLocationRequired");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "CartTokenRequired: returns validation error")]
    public void CartTokenRequired_HasCorrectCode()
    {
        var error = StockReservationResult.Errors.CartTokenRequired;
        error.Code.Should().Be("StockReservation.Cart.CartTokenRequired");
    }

    [Fact(DisplayName = "TtlOutOfRange: returns validation error referencing constant values")]
    public void TtlOutOfRange_ReferencesConstantValues()
    {
        var error = StockReservationResult.Errors.TtlOutOfRange;
        error.Message.Should().Contain(StockReservationConstant.Defaults.MinTtlMinutes.ToString());
        error.Message.Should().Contain(StockReservationConstant.Defaults.MaxTtlMinutes.ToString());
    }
}
