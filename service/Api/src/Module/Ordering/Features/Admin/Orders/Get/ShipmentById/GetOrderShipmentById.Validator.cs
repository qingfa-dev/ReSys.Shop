using FluentValidation;

namespace Module.Ordering.Features.Admin.Orders.Get.ShipmentById;

public static partial class GetOrderShipmentById
{
    public sealed class Validator : AbstractValidator<Query>
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
