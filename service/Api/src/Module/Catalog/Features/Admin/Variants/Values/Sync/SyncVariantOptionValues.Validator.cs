using Module.Catalog.Domain.Variants.Options;

namespace Module.Catalog.Features.Admin.Variants.Values.Sync;

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