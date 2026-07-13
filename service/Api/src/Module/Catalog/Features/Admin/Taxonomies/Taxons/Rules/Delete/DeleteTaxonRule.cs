using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;

using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Delete;

public static partial class DeleteTaxonRule
{
    public sealed record Command(Guid TaxonomyId, Guid TaxonId, Guid RuleId) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IAutoClassificationService autoClassificationService,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Deletes a taxon rule, persists the removal, and triggers auto-classification regeneration if the taxon is automatic.
        /// </summary>
        /// <param name="command">The command containing taxonomy ID, taxon ID, and rule ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result indicating the rule was deleted.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var taxonomyId = command.TaxonomyId;
            var taxonId = command.TaxonId;
            var ruleId = command.RuleId;

            var taxon = await dbContext.Set<Taxon>()
                .FirstOrDefaultAsync(x => x.Id == taxonId && x.TaxonomyId == taxonomyId, cancellationToken);
            if (taxon is null)
                return TaxonResult.Errors.NotFound;

            var rule = await dbContext.Set<TaxonRule>()
                .FirstOrDefaultAsync(x => x.Id == ruleId && x.TaxonId == taxonId, cancellationToken);
            if (rule is null)
                return TaxonRuleResult.Errors.NotFound;

            dbContext.Set<TaxonRule>().Remove(rule);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record taxon rule deletion event for audit trail
            TaxonRuleLoggers.Deleted(logger, ruleId, taxonId);

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

            return new Response(ruleId);
        }
    }
}