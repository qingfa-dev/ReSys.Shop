using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.UpdateShipmentState;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.UpdateShipmentState;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "UpdateOrderShipmentState")]
public class UpdateOrderShipmentStateValidatorTests
{
    private readonly UpdateOrderShipmentState.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should reject a null FulfillmentState")]
    public void Validate_WhenFulfillmentStateNull_ShouldHaveError()
    {
        var command = new UpdateOrderShipmentState.Command(
            Guid.NewGuid(),
            new UpdateOrderShipmentState.Request { FulfillmentState = null });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.FulfillmentState)
            .WithErrorCode(OrderResult.Errors.InvalidShipmentState.Code);
    }

    [Fact(DisplayName = "Validator: Should reject an undefined FulfillmentState value")]
    public void Validate_WhenFulfillmentStateUndefined_ShouldHaveError()
    {
        var command = new UpdateOrderShipmentState.Command(
            Guid.NewGuid(),
            new UpdateOrderShipmentState.Request { FulfillmentState = (OrderFulfillmentState)999 });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.FulfillmentState)
            .WithErrorCode(OrderResult.Errors.InvalidShipmentState.Code);
    }

    [Fact(DisplayName = "Validator: Should pass with a defined FulfillmentState")]
    public void Validate_WhenFulfillmentStateDefined_ShouldPass()
    {
        var command = new UpdateOrderShipmentState.Command(
            Guid.NewGuid(),
            new UpdateOrderShipmentState.Request { FulfillmentState = OrderFulfillmentState.Pending });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Request.FulfillmentState);
    }
}
