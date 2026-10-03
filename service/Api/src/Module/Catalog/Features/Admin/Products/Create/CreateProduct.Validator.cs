using Module.Catalog.Features.Admin.Shared.Validators;

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