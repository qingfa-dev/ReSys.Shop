using FluentValidation;
using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shipments.Get.ById;

public static partial class GetShipmentById
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode(ShipmentResult.Errors.NotFound(Guid.Empty).Code)
                .WithMessage("Shipment ID is required.");
        }
    }
}
