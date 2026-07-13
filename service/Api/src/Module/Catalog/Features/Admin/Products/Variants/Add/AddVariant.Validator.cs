using Module.Catalog.Features.Admin.Products.Variants.Shared.Validators;

namespace Module.Catalog.Features.Admin.Products.Variants.Add;

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