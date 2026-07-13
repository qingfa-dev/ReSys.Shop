using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Validations;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Assign;

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