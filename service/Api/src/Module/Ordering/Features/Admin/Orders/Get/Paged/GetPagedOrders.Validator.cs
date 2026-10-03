using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Get.Paged;

public static partial class GetPagedOrders
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Parameters)
                .NotNull()
                .WithErrorCode(OrderResult.Errors.ParametersRequired.Code)
                .WithMessage(OrderResult.Errors.ParametersRequired.Message);
        }
    }
}