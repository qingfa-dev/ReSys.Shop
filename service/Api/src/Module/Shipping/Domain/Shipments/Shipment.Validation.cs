namespace Module.Shipping.Domain.Shipments;

public static class ShipmentValidation
{
    // Validate: Shipment number must not be empty or exceed max length
    public static IRuleBuilderOptions<T, string> ApplyNumberRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(ShipmentConstant.Constraints.MaxNumberLength)
            .WithErrorCode(ShipmentResult.Errors.NumberTooLong.Code)
            .WithMessage(ShipmentResult.Errors.NumberTooLong.Description);
    }

    // Validate: Tracking number must not exceed max length
    public static IRuleBuilderOptions<T, string?> ApplyTrackingRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ShipmentConstant.Constraints.MaxTrackingLength)
            .WithErrorCode(ShipmentResult.Errors.TrackingTooLong.Code)
            .WithMessage(ShipmentResult.Errors.TrackingTooLong.Description);
    }

    // Validate: State transition conforms to Pending->Ready->Shipped; Pending|Ready->Canceled
    //           (Ruby SDK shipment.rb state machine alignment)
    public static bool IsValidTransition(ShipmentState current, ShipmentState next)
    {
        return (current, next) switch
        {
            (ShipmentState.Pending, ShipmentState.Ready) => true,
            (ShipmentState.Pending, ShipmentState.Canceled) => true,
            (ShipmentState.Ready, ShipmentState.Shipped) => true,
            (ShipmentState.Ready, ShipmentState.Canceled) => true,
            _ => false
        };
    }
}