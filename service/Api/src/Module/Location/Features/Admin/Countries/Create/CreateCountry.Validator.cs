using Module.Location.Features.Shared.Countries.Validators;

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