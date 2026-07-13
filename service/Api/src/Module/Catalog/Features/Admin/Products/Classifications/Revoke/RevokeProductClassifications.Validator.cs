using Module.Catalog.Features.Admin.Products.Classifications.Shared.Validations;

namespace Module.Catalog.Features.Admin.Products.Classifications.Revoke;

public static partial class RevokeProductClassifications
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Items)
                .ApplyProductClassificationAssignmentItemRules();
        }
    }
}