using FluentValidation;

namespace Module.Ordering.Features.Admin.Orders.Update.Shipment;

public static partial class UpdateOrderShipment
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");

            RuleFor(x => x.ShipmentId)
                .NotEmpty()
                .WithErrorCode("Shipment.Id.Required")
                .WithMessage("Shipment ID is required.");
        }
    }
}
