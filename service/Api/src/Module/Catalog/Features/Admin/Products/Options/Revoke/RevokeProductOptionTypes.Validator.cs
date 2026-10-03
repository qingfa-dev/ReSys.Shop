using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.Catalog.Features.Admin.Products.Options.Revoke;

public static partial class RevokeProductOptionTypes
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