using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.Catalog.Features.Admin.Optiontypes.Values.Update;

public static partial class UpdateOptionValue
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Request)
                .ApplyOptionValueRequestRules();
        }
    }
}