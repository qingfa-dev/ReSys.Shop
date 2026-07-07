namespace Module.Ordering.Features.Admin.Orders.Get.ById;

public static partial class GetOrderById
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
