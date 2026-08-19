using Module.Location.Features.Shared.States.Validators;

namespace Module.Location.Features.Admin.States.Update;

public static partial class UpdateState
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(expression: x => x.Request)
                .ApplyStateParametersRules();
        }
    }
}