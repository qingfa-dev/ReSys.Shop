using Module.Ordering.Features.Admin.Orders.Shared.Validators;

namespace Module.Ordering.Features.Admin.Orders.Create;

public static partial class CreateOrder
{
    /// <summary>Validates the CreateOrder command — request parameters must be valid.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Order request parameters (currency, email format).
            RuleFor(x => x.Request).ApplyOrderParametersRules();
        }
    }
}