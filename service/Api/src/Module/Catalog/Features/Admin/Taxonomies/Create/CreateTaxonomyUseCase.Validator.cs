using Module.Catalog.Features.Admin.Shared.Validators;

namespace Module.Catalog.Features.Admin.Taxonomies.Create;

public static partial class CreateTaxonomy
{
    // Validator:
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyTaxonomyParametersRules();
        }
    }
}