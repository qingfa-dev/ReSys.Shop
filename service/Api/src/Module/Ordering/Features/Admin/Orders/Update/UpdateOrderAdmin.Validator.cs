using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Update;

public static partial class UpdateOrderAdmin
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode(OrderResult.Errors.IdRequired.Code)
                .WithMessage(OrderResult.Errors.IdRequired.Message);

            When(x => x.Request.Email is not null, () =>
            {
                RuleFor(x => x.Request.Email).EmailAddress()
                    .WithErrorCode("Order.Email.Invalid")
                    .WithMessage("Email address is not valid.");
            });
        }
    }
}
