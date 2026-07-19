using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;

// Boundary: Features → Domain — hierarchy service operates on taxon domain entities via nested set model
/// <summary>Manages the modified preorder tree traversal (MPTT) hierarchy for taxons — rebuild, validate, and regenerate permalinks.</summary>
public partial class TaxonHierarchyService : ITaxonHierarchyService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TaxonHierarchyService> _logger;

    public TaxonHierarchyService(
        ApplicationDbContext dbContext,
        ILogger<TaxonHierarchyService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
}