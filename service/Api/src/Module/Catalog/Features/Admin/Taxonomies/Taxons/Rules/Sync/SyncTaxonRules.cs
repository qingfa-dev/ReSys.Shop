using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;

using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Mappings;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Sync;

/// <summary>
/// Defines the use case for synchronizing taxon rules.
/// </summary>
public static partial class SyncTaxonRules
{
    public sealed record Command(Guid TaxonId, Request Request) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(
        IApplicationDbContext dbContext,
        IAutoClassificationService autoClassificationService,
        ILogger<PagedQueryHandler> logger)
        : IPagedQueryHandler<Command, Response>
    {
        /// <summary>
        /// Synchronises taxon rules — creates, updates, and removes rules to match the
        /// incoming set, then triggers auto-classification if the taxon is automatic.
        /// </summary>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.TaxonomyId!=Guid.Empty && command.TaxonId!=Guid.Empty, post=result!=null, throws=DbUpdateException
        public async Task<PagedResult<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var taxonId = command.TaxonId;
            var request = command.Request;

            // Validate: Parent taxon must exist
            var taxon = await dbContext.Set<Taxon>()
                .FirstOrDefaultAsync(x => x.Id == taxonId, cancellationToken);
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

            var mapped = updatedRules.Select(r => r.MapToListItem<Response>()).ToList();
            return PagedResult<Response>.Create(mapped, 1, Math.Max(1, mapped.Count), mapped.Count);
        }
    }
}