using FluentValidation;

namespace Module.Ordering.Features.Admin.Orders.Update;

public static partial class UpdateOrderAdmin
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            When(x => x.Request.Email is not null, () =>
            {
                RuleFor(x => x.Request.Email).EmailAddress()
                    .WithErrorCode("Order.Email.Invalid")
                    .WithMessage("Email address is not valid.");
            });
        }
    }
}
