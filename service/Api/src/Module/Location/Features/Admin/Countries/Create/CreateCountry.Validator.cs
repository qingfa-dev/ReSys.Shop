using Module.Location.Features.Admin.Countries.Shared.Validators;

namespace Module.Location.Features.Admin.Countries.Create;

public static partial class CreateCountry
{
    // CommandValidator
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(expression: x => x.Request)
                .ApplyCountryParametersRules();
        }
    }
}