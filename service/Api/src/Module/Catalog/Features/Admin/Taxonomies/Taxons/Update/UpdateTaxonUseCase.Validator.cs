using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Validators;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Update;

public static partial class UpdateTaxon
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