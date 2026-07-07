namespace Module.Ordering.Features.Storefront.Orders.Cancel;

public static partial class CancelOrder
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
