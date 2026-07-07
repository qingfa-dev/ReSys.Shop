using FluentValidation;
using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shipments.UpdateTracking;

public static partial class UpdateShipmentTracking
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode(ShipmentResult.Errors.NotFound(Guid.Empty).Code)
                .WithMessage("Shipment ID is required.");

            RuleFor(x => x.Request.Tracking)
                .NotEmpty()
                .MaximumLength(ShipmentConstant.Constraints.MaxTrackingLength)
                .WithErrorCode(ShipmentResult.Errors.TrackingTooLong.Code)
                .WithMessage(ShipmentResult.Errors.TrackingTooLong.Description);
        }
    }
}
