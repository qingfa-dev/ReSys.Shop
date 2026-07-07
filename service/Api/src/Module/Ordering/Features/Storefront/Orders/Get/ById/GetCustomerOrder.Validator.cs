namespace Module.Ordering.Features.Storefront.Orders.Get.ById;

public static partial class GetCustomerOrder
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
