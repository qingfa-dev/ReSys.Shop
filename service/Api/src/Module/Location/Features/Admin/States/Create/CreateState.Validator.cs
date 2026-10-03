using Module.Location.Features.Shared.States.Validators;

namespace Module.Location.Features.Admin.States.Create;

public static partial class CreateState
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