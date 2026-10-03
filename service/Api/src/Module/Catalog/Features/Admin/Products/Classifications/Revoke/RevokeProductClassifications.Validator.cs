using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Revoke;

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