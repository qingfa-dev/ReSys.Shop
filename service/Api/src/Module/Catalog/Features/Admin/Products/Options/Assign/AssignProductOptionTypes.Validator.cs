using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.Catalog.Features.Admin.Products.Options.Assign;

public static partial class AssignProductOptionTypes
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