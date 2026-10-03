using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Delete;

public static partial class DeleteOrder
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode(OrderResult.Errors.IdRequired.Code)
                .WithMessage(OrderResult.Errors.IdRequired.Message);
        }
    }
}