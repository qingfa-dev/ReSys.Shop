using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.Catalog.Features.Admin.Optiontypes.Values.Create;

public static partial class CreateOptionValue
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyOptionValueRequestRules();
        }
    }
}