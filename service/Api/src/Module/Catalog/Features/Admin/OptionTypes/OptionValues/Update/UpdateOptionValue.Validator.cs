using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Validators;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Update;

public static partial class UpdateOptionValue
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OptionTypeId).NotEmpty();
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Request)
                .ApplyOptionValueParametersRules();
        }
    }
}
