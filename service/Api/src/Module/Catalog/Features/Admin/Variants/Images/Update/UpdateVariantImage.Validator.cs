using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.Catalog.Features.Admin.Variants.Images.Update;

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