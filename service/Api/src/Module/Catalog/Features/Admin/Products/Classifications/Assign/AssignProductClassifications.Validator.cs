using Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Validations;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Assign;

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