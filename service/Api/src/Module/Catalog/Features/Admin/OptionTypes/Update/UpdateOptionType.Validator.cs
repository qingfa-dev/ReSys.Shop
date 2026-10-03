using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.Catalog.Features.Admin.OptionTypes.Update;

public static partial class UpdateOptionType
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Request)
                .ApplyOptionTypeParametersRules();
        }
    }
}