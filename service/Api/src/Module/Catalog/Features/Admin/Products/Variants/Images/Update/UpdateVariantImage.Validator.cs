using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Validators;

namespace Module.Catalog.Features.Admin.Products.Update;

public static partial class UpdateVariantImage
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x)
                .SetValidator(new VariantImageValidator.UpdateImageRequestValidator());
        }
    }
}
