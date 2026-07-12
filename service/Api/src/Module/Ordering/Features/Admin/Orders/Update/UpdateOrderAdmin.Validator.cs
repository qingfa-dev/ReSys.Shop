using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Update;

public static partial class UpdateOrderAdmin
{
    /// <summary>Validates the UpdateOrderAdmin command — order ID required; email validated when provided.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Order ID must not be empty.
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode(OrderResult.Failure.IdRequired.Code)
                .WithMessage(OrderResult.Failure.IdRequired.Message);

            // Validate: Email format when a new email is provided.
            When(x => x.Request.Email is not null, () =>
            {
                RuleFor(x => x.Request.Email).EmailAddress()
                    .WithErrorCode("Order.Email.Invalid")
                    .WithMessage("Email address is not valid.");
            });
        }
    }
}
