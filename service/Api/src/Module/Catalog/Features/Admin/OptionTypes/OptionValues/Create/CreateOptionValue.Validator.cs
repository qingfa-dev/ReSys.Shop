using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Validators;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Create;

public static partial class CreateOptionValue
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OptionTypeId).NotEmpty();

            RuleFor(x => x.Request)
                .ApplyOptionValueParametersRules();
        }
    }
}