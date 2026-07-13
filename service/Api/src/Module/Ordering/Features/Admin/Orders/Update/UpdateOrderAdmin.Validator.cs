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
                .WithErrorCode(OrderResult.Errors.IdRequired.Code)
                .WithMessage(OrderResult.Errors.IdRequired.Message);

            When(x => x.Request.Email is not null, () =>
            {
                RuleFor(x => x.Request.Email).ApplyEmailRules();
            });
        }
    }
}