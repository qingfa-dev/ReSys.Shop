namespace Module.Ordering.Features.Admin.Orders.UpdateStatus;

public static partial class UpdateOrderStatus
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).NotNull();
            RuleFor(x => x.Request.Status).IsInEnum();
        }
    }
}
