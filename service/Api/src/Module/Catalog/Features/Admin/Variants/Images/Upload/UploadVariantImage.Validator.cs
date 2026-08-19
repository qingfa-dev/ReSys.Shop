using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.Catalog.Features.Admin.Variants.Images.Upload;

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