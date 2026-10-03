using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shipments.UpdateStatus;

public static partial class UpdateShipmentStatus
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Status)
                .IsInEnum()
                .WithMessage("A valid shipment status is required.");

            RuleFor(x => x.Request.TrackingNumber)
                .ApplyTrackingNumberRules()
                .When(x => x.Request.TrackingNumber is not null);
        }
    }
}
