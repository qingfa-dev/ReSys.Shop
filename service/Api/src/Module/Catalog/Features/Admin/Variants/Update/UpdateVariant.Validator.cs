using Module.Catalog.Features.Admin.Variants.Shared.Validators;

namespace Module.Catalog.Features.Admin.Variants.Update;

public static partial class UpdateVariant
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyVariantParametersRules();
        }
    }
}