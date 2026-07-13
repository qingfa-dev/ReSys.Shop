using Module.Catalog.Features.Admin.Products.Shared.Validation;

namespace Module.Catalog.Features.Admin.Products.Create;

public static partial class CreateProduct
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