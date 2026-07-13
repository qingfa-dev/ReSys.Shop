namespace Module.Ordering.Features.Admin.Orders.Get.Paged;

public static partial class GetPagedOrders
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Parameters)
                .NotNull()
                .WithErrorCode("Order.Parameters.Required")
                .WithMessage("Query parameters are required.");
        }
    }
}