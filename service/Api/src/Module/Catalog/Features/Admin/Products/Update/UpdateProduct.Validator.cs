using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.Catalog.Features.Admin.Products.Update;

public static partial class UpdateProduct
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyProductParametersRules();
        }
    }
}