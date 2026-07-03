using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification;

/// <summary>
/// Orchestrates automatic product ↔ taxon classification.
/// </summary>
public sealed class AutoClassificationService(
    IApplicationDbContext dbContext,
    ITaxonRuleEvaluator ruleEvaluator) : IAutoClassificationService
{
    private const int BatchSize = 500;

    /// <inheritdoc />
    public async Task RegenerateForTaxonAsync(Guid taxonId, CancellationToken ct = default)
    {
        // Fetch: Target taxon with its rules
        var taxon = await dbContext.Set<Taxon>()
            .Include(t => t.TaxonRules)
            .FirstOrDefaultAsync(t => t.Id == taxonId, ct);

        if (taxon == null || !taxon.Automatic) return;

        // Fetch: Existing automatic classifications for this taxon
        var existingClassifications = await dbContext.Set<Classification>()
            .Where(c => c.TaxonId == taxonId && c.IsAutomatic)
            .ToListAsync(ct);

        var existingProductIds = existingClassifications
            .Where(c => c.ProductId.HasValue)
            .Select(c => c.ProductId!.Value)
            .ToHashSet();

        // Process: All products in batches to evaluate rules
        int skip = 0;
        while (true)
        {
            var products = await dbContext.Set<Product>()
                .Include(p => p.Variants)
                .OrderBy(p => p.Id)
                .Skip(skip)
                .Take(BatchSize)
                .ToListAsync(ct);

            if (products.Count == 0) break;

            foreach (var product in products)
            {
                bool matches = ruleEvaluator.Evaluate(product, taxon);
                bool hasClassification = existingProductIds.Contains(product.Id);

                if (matches && !hasClassification)
                {
                    // Add: New automatic classification
                    var classificationResult = ClassificationExtensions.Create(product.Id, taxonId, isAutomatic: true);
                    if (classificationResult.IsFailure)
                        continue;

                    await dbContext.Set<Classification>().AddAsync(classificationResult.Value, ct);
                }
                else if (!matches && hasClassification)
                {
                    // Remove: Stale automatic classification
                    var stale = existingClassifications.FirstOrDefault(c => c.ProductId == product.Id);
                    if (stale != null)
                    {
                        dbContext.Set<Classification>().Remove(stale);
                    }
                }
            }

            skip += BatchSize;
            if (products.Count < BatchSize) break;
        }

        // Finalize: Clear dirty flag and commit
        taxon.MarkedForRegenerateTaxonProducts = false;
        await dbContext.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task RegenerateForProductAsync(Guid productId, CancellationToken ct = default)
    {
        // Fetch: Target product with its variants
        var product = await dbContext.Set<Product>()
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == productId, ct);

        if (product == null) return;

        // Fetch: All automatic taxons
        var automaticTaxons = await dbContext.Set<Taxon>()
            .Include(t => t.TaxonRules)
            .Where(t => t.Automatic)
            .ToListAsync(ct);

        // Fetch: Existing automatic classifications for this product
        var existingClassifications = await dbContext.Set<Classification>()
            .Where(c => c.ProductId == productId && c.IsAutomatic)
            .ToListAsync(ct);

        var existingTaxonIds = existingClassifications
            .Where(c => c.TaxonId.HasValue)
            .Select(c => c.TaxonId!.Value)
            .ToHashSet();

        foreach (var taxon in automaticTaxons)
        {
            bool matches = ruleEvaluator.Evaluate(product, taxon);
            bool hasClassification = existingTaxonIds.Contains(taxon.Id);

            if (matches && !hasClassification)
            {
                // Add: New automatic classification
                var classificationResult = ClassificationExtensions.Create(productId, taxon.Id, isAutomatic: true);
                if (classificationResult.IsFailure)
                    return;

                await dbContext.Set<Classification>().AddAsync(classificationResult.Value, ct);
            }
            else if (!matches && hasClassification)
            {
                // Remove: Stale automatic classification
                var stale = existingClassifications.FirstOrDefault(c => c.TaxonId == taxon.Id);
                if (stale != null)
                {
                    dbContext.Set<Classification>().Remove(stale);
                }
            }
        }

        // Finalize: Clear dirty flag and commit
        product.MarkedForRegenerateTaxonProducts = false;
        await dbContext.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task RegenerateDirtyAsync(CancellationToken ct = default)
    {
        // 1. Process: Taxons marked for regeneration
        var dirtyTaxonIds = await dbContext.Set<Taxon>()
            .Where(t => t.MarkedForRegenerateTaxonProducts)
            .Select(t => t.Id)
            .ToListAsync(ct);

        foreach (var taxonId in dirtyTaxonIds)
        {
            await RegenerateForTaxonAsync(taxonId, ct);
        }

        // 2. Process: Products marked for regeneration
        var dirtyProductIds = await dbContext.Set<Product>()
            .Where(p => p.MarkedForRegenerateTaxonProducts)
            .Select(p => p.Id)
            .ToListAsync(ct);

        foreach (var productId in dirtyProductIds)
        {
            await RegenerateForProductAsync(productId, ct);
        }
    }
}
