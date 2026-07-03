using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Validators;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Upload;

public static partial class UploadVariantImage
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x)
                .SetValidator(new VariantImageValidator.UploadImageRequestValidator());
        }
    }
}
