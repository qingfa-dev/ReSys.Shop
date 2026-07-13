namespace Module.Ordering.Features.Admin.Orders.UpdateShipAddress;

public static partial class UpdateOrderShipAddress
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");

            RuleFor(x => x.Request)
                .NotNull()
                .WithErrorCode("Order.Request.Required")
                .WithMessage("Request body is required.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request!.AddressId)
                    .NotEmpty()
                    .WithErrorCode("Order.AddressId.Required")
                    .WithMessage("Address ID is required.");
            });
        }
    }
}