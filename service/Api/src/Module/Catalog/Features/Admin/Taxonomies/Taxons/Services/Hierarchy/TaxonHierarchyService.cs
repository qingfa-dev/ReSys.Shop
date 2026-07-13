using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;

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