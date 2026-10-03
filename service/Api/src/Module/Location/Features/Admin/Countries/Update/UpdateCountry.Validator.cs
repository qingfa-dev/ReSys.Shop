using Module.Location.Features.Shared.Countries.Validators;

namespace Module.Location.Features.Admin.Countries.Update;

public static partial class UpdateCountry
{
    // ============ COMMAND VALIDATOR ============
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(expression: x => x.Request)
                .ApplyCountryParametersRules();
        }
    }

}