using Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Validations;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Sync;

public static partial class SyncProductClassifications
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