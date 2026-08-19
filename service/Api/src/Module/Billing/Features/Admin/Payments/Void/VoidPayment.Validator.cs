namespace Module.Billing.Features.Admin.Payments.Void;

public static partial class VoidPayment
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}