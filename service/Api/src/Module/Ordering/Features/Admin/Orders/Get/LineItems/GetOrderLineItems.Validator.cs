using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Get.LineItems;

public static partial class GetOrderLineItems
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode(OrderResult.Errors.IdRequired.Code)
                .WithMessage(OrderResult.Errors.IdRequired.Message);

            RuleFor(x => x.Parameters)
                .NotNull()
                .WithErrorCode(OrderResult.Errors.ParametersRequired.Code)
                .WithMessage(OrderResult.Errors.ParametersRequired.Message);
        }
    }
}