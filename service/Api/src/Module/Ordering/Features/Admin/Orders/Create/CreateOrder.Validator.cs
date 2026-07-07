using Module.Ordering.Features.Admin.Orders.Shared.Validators;

namespace Module.Ordering.Features.Admin.Orders.Create;

public static partial class CreateOrder
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).ApplyOrderParametersRules();
        }
    }
}
