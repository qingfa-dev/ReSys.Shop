using Module.Catalog.Domain.Products.Variants.Options;

namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Get;

public static partial class GetVariantOptionValues
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.VariantId)
                .ApplyVariantIdRules();
        }
    }
}
