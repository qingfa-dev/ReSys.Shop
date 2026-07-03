using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;

using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Mappings;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Sync;

public static partial class SyncTaxonRules
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
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var taxonomyId = command.TaxonomyId;
            var taxonId = command.TaxonId;
            var request = command.Request;

            var taxon = await dbContext.Set<Taxon>()
                .FirstOrDefaultAsync(x => x.Id == taxonId && x.TaxonomyId == taxonomyId, cancellationToken);
            if (taxon is null)
                return TaxonResult.Errors.NotFound;

            var existingRules = await dbContext.Set<TaxonRule>()
                .Where(x => x.TaxonId == taxonId)
                .ToListAsync(cancellationToken);

            var incomingIds = request.Rules
                .Where(r => r.Id.HasValue)
                .Select(r => r.Id!.Value)
                .ToHashSet();

            foreach (var item in request.Rules)
            {
                if (item.Id.HasValue)
                {
                    var existing = existingRules.FirstOrDefault(r => r.Id == item.Id.Value);
                    if (existing is not null)
                    {
                        item.ToEntity(existing);
                    }
                }
                else
                {
                    var rule = item.ToEntity(taxonId);
                    dbContext.Set<TaxonRule>().Add(rule);
                }
            }

            var toRemove = existingRules.Where(r => !incomingIds.Contains(r.Id)).ToList();
            foreach (var rule in toRemove)
            {
                dbContext.Set<TaxonRule>().Remove(rule);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record taxon rules sync event for audit trail
            TaxonRuleLoggers.Updated(logger, Guid.Empty, taxonId);

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

            var updatedRules = await dbContext.Set<TaxonRule>()
                .Where(x => x.TaxonId == taxonId)
                .OrderBy(x => x.Type)
                .ToListAsync(cancellationToken);

            var mapped = updatedRules.Select(r => r.MapToListItem<TaxonRuleItem>()).ToList();
            return new Response { Rules = mapped };
        }
    }
}
