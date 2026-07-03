using Module.Catalog.Domain.Products.Variants.Options;

namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Revoke;

public static partial class RevokeVariantOptionValues
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.OptionValueIds)
                .NotNull()
                .WithErrorCode(OptionValueVariantResult.Errors.OptionValueIdsRequired.Code)
                .WithMessage(OptionValueVariantResult.Errors.OptionValueIdsRequired.Message)
                .NotEmpty()
                .WithErrorCode(OptionValueVariantResult.Errors.OptionValueIdsEmpty.Code)
                .WithMessage(OptionValueVariantResult.Errors.OptionValueIdsEmpty.Message);
        }
    }
}
