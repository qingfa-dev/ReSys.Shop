namespace Module.Shipping.Domain.Shipments;

public static class ShipmentValidation
{
    // Validate: Tracking number must not exceed max length
    public static IRuleBuilderOptions<T, string?> ApplyTrackingNumberRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ShipmentConstant.Constraints.MaxTrackingLength)
            .WithErrorCode(ShipmentResult.Errors.TrackingNumberTooLong.Code)
            .WithMessage(ShipmentResult.Errors.TrackingNumberTooLong.Message);
    }

    // Validate: State transition conforms to Pending->Ready->Shipped; Pending|Ready->Canceled
    //           (Ruby SDK shipment.rb state machine alignment)
    public static bool IsValidTransition(ShipmentStatus current, ShipmentStatus next)
    {
        return (current, next) switch
        {
            (ShipmentStatus.Pending, ShipmentStatus.Ready) => true,
            (ShipmentStatus.Pending, ShipmentStatus.Canceled) => true,
            (ShipmentStatus.Ready, ShipmentStatus.Shipped) => true,
            (ShipmentStatus.Ready, ShipmentStatus.Canceled) => true,
            _ => false
        };
    }
}