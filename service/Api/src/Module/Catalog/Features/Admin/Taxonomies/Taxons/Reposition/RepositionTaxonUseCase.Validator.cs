using Module.Catalog.Domain.Taxons;

namespace Module.Catalog.Features.Admin.Taxons.Reposition;

public static partial class RepositionTaxon
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).NotNull();
            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.Position).ApplyPositionRules();
                RuleFor(x => x.Request.ParentId).ApplyTaxonParentIdRules();
            });
        }
    }
}