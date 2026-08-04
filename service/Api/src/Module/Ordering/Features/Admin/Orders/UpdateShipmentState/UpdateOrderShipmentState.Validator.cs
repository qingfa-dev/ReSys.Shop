using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.UpdateShipmentState;

public static partial class UpdateOrderShipmentState
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode(OrderResult.Errors.IdRequired.Code)
                .WithMessage(OrderResult.Errors.IdRequired.Message);

            RuleFor(x => x.Request)
                .NotNull()
                .WithErrorCode(OrderResult.Errors.RequestRequired.Code)
                .WithMessage(OrderResult.Errors.RequestRequired.Message);

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request!.ShipmentState)
                    .NotEmpty()
                    .WithMessage("Shipment state is required.");

                RuleFor(x => x.Request!.ShipmentState)
                    .Must(state => new[]
                    {
                        OrderConstant.ShipmentState.Pending,
                        OrderConstant.ShipmentState.Delivered,
                        OrderConstant.ShipmentState.Partial,
                        OrderConstant.ShipmentState.Ready,
                        OrderConstant.ShipmentState.Backorder,
                        OrderConstant.ShipmentState.Canceled,
                    }.Contains(state))
                    .WithErrorCode(OrderResult.Errors.InvalidShipmentState.Code)
                    .WithMessage(OrderResult.Errors.InvalidShipmentState.Message);
            });
        }
    }
}
