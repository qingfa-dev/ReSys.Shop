using Module.Catalog.Features.Admin.Taxonomies.Shared.Validators;

namespace Module.Catalog.Features.Admin.Taxonomies.Update;

public static partial class UpdateTaxonomy
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Request)
                .ApplyTaxonomyParametersRules();
        }
    }
}