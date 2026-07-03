using Module.Catalog.Features.Admin.Products.Classifications.Shared.Validations;

namespace Module.Catalog.Features.Admin.Products.Classifications.Assign;

public static partial class AssignProductClassifications
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
