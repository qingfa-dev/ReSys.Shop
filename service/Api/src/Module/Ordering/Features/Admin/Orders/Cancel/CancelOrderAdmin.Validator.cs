using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Cancel;

public static partial class CancelOrderAdmin
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
                RuleFor(x => x.Request.Reason)
                    .MaximumLength(500)
                    .WithErrorCode(OrderResult.Errors.ReasonTooLong.Code)
                    .WithMessage(OrderResult.Errors.ReasonTooLong.Message);
            });
        }
    }
}