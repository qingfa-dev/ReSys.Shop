using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Validations;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Sync;

public static partial class SyncProductOptionTypes
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Items)
                .ApplyProductOptionTypeAssignmentItemRules();
        }
    }
}
