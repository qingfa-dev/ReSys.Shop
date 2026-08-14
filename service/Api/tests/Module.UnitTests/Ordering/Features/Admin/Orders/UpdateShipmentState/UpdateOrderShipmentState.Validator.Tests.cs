using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.UpdateShipmentState;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.UpdateShipmentState;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "UpdateOrderShipmentState")]
public class UpdateOrderShipmentStateValidatorTests
{
    private readonly UpdateOrderShipmentState.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should reject a null ShipmentState")]
    public void Validate_WhenShipmentStateNull_ShouldHaveError()
    {
        var command = new UpdateOrderShipmentState.Command(
            Guid.NewGuid(),
            new UpdateOrderShipmentState.Request { ShipmentState = null });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.ShipmentState)
            .WithErrorCode(OrderResult.Errors.InvalidShipmentState.Code);
    }

    [Fact(DisplayName = "Validator: Should reject an undefined ShipmentState value")]
    public void Validate_WhenShipmentStateUndefined_ShouldHaveError()
    {
        var command = new UpdateOrderShipmentState.Command(
            Guid.NewGuid(),
            new UpdateOrderShipmentState.Request { ShipmentState = (OrderShipmentState)999 });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.ShipmentState)
            .WithErrorCode(OrderResult.Errors.InvalidShipmentState.Code);
    }

    [Fact(DisplayName = "Validator: Should pass with a defined ShipmentState")]
    public void Validate_WhenShipmentStateDefined_ShouldPass()
    {
        var command = new UpdateOrderShipmentState.Command(
            Guid.NewGuid(),
            new UpdateOrderShipmentState.Request { ShipmentState = OrderShipmentState.Pending });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Request.ShipmentState);
    }
}
