using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxons.Shared.Validators;

namespace Module.Catalog.Features.Admin.Taxons.Create;

public static partial class CreateTaxon
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyTaxonomyParametersRules();
        }
    }
}