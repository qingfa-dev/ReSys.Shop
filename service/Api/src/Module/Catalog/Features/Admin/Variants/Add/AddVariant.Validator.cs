using Module.Catalog.Features.Admin.Variants.Shared.Validators;

namespace Module.Catalog.Features.Admin.Variants.Add;

public static partial class AddVariant
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