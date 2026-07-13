namespace Module.Ordering.Features.Admin.Orders.Delete;

public static partial class DeleteOrder
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");
        }
    }
}
