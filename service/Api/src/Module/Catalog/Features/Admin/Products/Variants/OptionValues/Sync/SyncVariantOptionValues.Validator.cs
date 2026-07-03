using Module.Catalog.Domain.Products.Variants.Options;

namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Sync;

public static partial class SyncVariantOptionValues
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.OptionValueIds)
                .NotNull()
                .WithErrorCode(OptionValueVariantResult.Errors.OptionValueIdsRequired.Code)
                .WithMessage(OptionValueVariantResult.Errors.OptionValueIdsRequired.Message);
        }
    }
}
