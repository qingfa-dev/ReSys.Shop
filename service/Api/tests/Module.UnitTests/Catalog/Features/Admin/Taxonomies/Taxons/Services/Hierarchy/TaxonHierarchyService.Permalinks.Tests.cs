using System.Reflection;

using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonHierarchy")]
public class TaxonHierarchyPermalinksTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<TaxonHierarchyService>> _loggerMock;
    private readonly TaxonHierarchyService _service;

    public TaxonHierarchyPermalinksTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<TaxonHierarchyService>>();

        _service = new TaxonHierarchyService(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static void InvokePrivateStaticMethod(string methodName, params object?[] args)
    {
        var method = typeof(TaxonHierarchyService).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null) throw new InvalidOperationException($"Method {methodName} not found.");
        method.Invoke(null, args);
    }

    [Fact(DisplayName = "RegeneratePermalinks: Should correctly update permalinks and pretty names")]
    public async Task RegeneratePermalinks_ShouldCorrectlyUpdate()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Clothing", "Clothing", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Mens", "Mens", null, 0, "mens", null, null, null, false, null, null, false, null, null).Value;
        var child = TaxonMethod.Create(taxonomy.Id, root.Id, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        
        // Correct nested sets (RegeneratePermalinks depends on them)
        root.Lft = 1; root.Rgt = 4;
        child.Lft = 2; child.Rgt = 3;

        // Garbage permalinks
        root.Permalink = "old-root";
        child.Permalink = "old-child";

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, child);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _service.RegeneratePermalinksAsync(taxonomy.Id, ct: ct);

        // Assert (Weak)
        result.IsSuccess.Should().BeTrue();

        // Assert (Strong)
        _dbContext.ChangeTracker.Clear();
        var updatedRoot = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == root.Id, ct);
        var updatedChild = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == child.Id, ct);

        updatedRoot.Permalink.Should().Be("clothing/mens");
        updatedRoot.PrettyName.Should().Be("Mens");

        updatedChild.Permalink.Should().Be("clothing/mens/shirts");
        updatedChild.PrettyName.Should().Be("Mens -> Shirts");
    }

    [Fact(DisplayName = "RegeneratePermalinks: Should correctly update subtree only")]
    public async Task RegeneratePermalinks_ShouldCorrectlyUpdate_Subtree()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Clothing", "Clothing", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Mens", "Mens", null, 0, "mens", null, null, null, false, null, null, false, null, null).Value;
        var child = TaxonMethod.Create(taxonomy.Id, root.Id, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        
        root.Lft = 1; root.Rgt = 4;
        child.Lft = 2; child.Rgt = 3;

        root.Permalink = "clothing/mens"; // Correct parent permalink
        child.Permalink = "old-child";

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, child);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act: Regenerate only for the child subtree (which is just the child itself)
        var result = await _service.RegeneratePermalinksAsync(taxonomy.Id, child.Id, ct: ct);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _dbContext.ChangeTracker.Clear();
        var updatedRoot = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == root.Id, ct);
        var updatedChild = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == child.Id, ct);

        updatedRoot.Permalink.Should().Be("clothing/mens"); // Should NOT change
        updatedChild.Permalink.Should().Be("clothing/mens/shirts"); // Should change
    }

    [Fact(DisplayName = "UpdatePermalinksInternal: Should wire up parents and update in-memory")]
    public void UpdatePermalinksInternal_ShouldWireUpAndUpdate()
    {
        // Arrange
        var taxoId = Guid.NewGuid();
        var root = TaxonMethod.Create(taxoId, null, "Mens", "Mens", null, 0, "mens", null, null, null, false, null, null, false, null, null).Value;
        var child = TaxonMethod.Create(taxoId, root.Id, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        
        var allTaxons = new List<Taxon> { root, child };
        var toUpdate = new List<Taxon> { root, child }; // Update both to ensure root is processed first

        // Act
        InvokePrivateStaticMethod("UpdatePermalinksInternal", "Clothing", allTaxons, toUpdate);

        // Assert
        child.Parent.Should().Be(root);
        root.Permalink.Should().Be("clothing/mens");
        child.Permalink.Should().Be("clothing/mens/shirts");
        child.PrettyName.Should().Be("Mens -> Shirts");
    }

    [Fact(DisplayName = "RegeneratePermalinks: Should use root slug when it matches taxonomy name (primary root)")]
    public async Task RegeneratePermalinks_ShouldHandlePrimaryRoot()
    {
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Categories", "Categories", null, 0, "categories", null, null, null, false, null, null, false, null, null).Value;
        
        root.Lft = 1; root.Rgt = 2;
        root.Permalink = "old";

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(root);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        var result = await _service.RegeneratePermalinksAsync(taxonomy.Id, ct: ct);

        result.IsSuccess.Should().BeTrue();

        _dbContext.ChangeTracker.Clear();
        var updated = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == root.Id, ct);
        updated.Permalink.Should().Be("categories");
        updated.PrettyName.Should().Be("Categories");
    }
}
