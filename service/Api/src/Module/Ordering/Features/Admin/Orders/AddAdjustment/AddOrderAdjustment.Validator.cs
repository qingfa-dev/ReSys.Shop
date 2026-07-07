namespace Module.Ordering.Features.Admin.Orders.AddAdjustment;

public static partial class AddOrderAdjustment
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).NotNull();
            RuleFor(x => x.Request.Label).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Request.Amount).NotEqual(0);
        }
    }
}
