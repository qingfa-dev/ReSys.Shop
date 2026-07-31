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

            RuleFor(x => x.Parameters.PageSize)
                .Must(value => value.HasValue && value.Value >= 1 && value.Value <= 100)
                .WithErrorCode("InvalidPageSize")
                .When(x => x.Parameters.PageSize.HasValue);
        }
    }
}