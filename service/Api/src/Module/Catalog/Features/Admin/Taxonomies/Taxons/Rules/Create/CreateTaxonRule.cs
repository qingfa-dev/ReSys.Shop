using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;

using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Mappings;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Create;

/// <summary>
/// Defines the use case for creating a new taxon rule.
/// </summary>
public static partial class CreateTaxonRule
{
    public sealed record Command(Guid TaxonomyId, Guid TaxonId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IAutoClassificationService autoClassificationService,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="command">The command containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var taxonomyId = command.TaxonomyId;
            var taxonId = command.TaxonId;
            var request = command.Request;

            var taxon = await dbContext.Set<Taxon>()
                .FirstOrDefaultAsync(x => x.Id == taxonId && x.TaxonomyId == taxonomyId, cancellationToken);
            if (taxon is null)
                return TaxonResult.Errors.NotFound;

            var rule = request.ToEntity(taxonId);

            dbContext.Set<TaxonRule>().Add(rule);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record taxon rule creation event for audit trail
            TaxonRuleLoggers.Created(logger, rule.Id, taxonId);

            // Call: Regenerate classification if taxon is automatic.
            if (taxon.Automatic)
            {
                // Log: Record classification regeneration start for observability
                TaxonRuleLoggers.ClassificationStarted(logger, taxonId);
                try
                {
                    await autoClassificationService.RegenerateForTaxonAsync(taxonId, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log: Record classification regeneration failure for observability
                    TaxonRuleLoggers.ClassificationFailed(logger, taxonId, ex.Message);
                }
            }

            return rule.MapToDetail<Response>();
        }
    }
}
