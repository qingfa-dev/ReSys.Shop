using Module.Catalog.Features.Admin.OptionTypes.Shared.Validators;

namespace Module.Catalog.Features.Admin.OptionTypes.Create;

public static partial class CreateOptionType
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyOptionTypeParametersRules();
        }
    }
}
