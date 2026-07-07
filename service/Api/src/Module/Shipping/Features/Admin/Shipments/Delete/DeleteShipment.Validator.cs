using FluentValidation;
using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shipments.Delete;

public static partial class DeleteShipment
{
    public sealed class Validator : AbstractValidator<Command>
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
