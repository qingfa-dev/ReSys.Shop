using System.Reflection;

using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Taxons.Services.Hierarchy;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonHierarchy")]
public class TaxonHierarchyRebuildTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<TaxonHierarchyService>> _loggerMock;
    private readonly TaxonHierarchyService _service;

    public TaxonHierarchyRebuildTests()
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

    private static T InvokePrivateStaticMethod<T>(string methodName, params object?[] args)
    {
        var method = typeof(TaxonHierarchyService).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null) throw new InvalidOperationException($"Method {methodName} not found.");
        return (T)method.Invoke(null, args)!;
    }

    [Fact(DisplayName = "RebuildHierarchy: Should correctly update coordinates and permalinks for full tree")]
    public async Task RebuildHierarchy_ShouldCorrectlyUpdate_FullTree()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Categories", "Categories", null, 0, "categories", null, null, null, false, null, null, false, null, null).Value;
        var child = TaxonMethod.Create(taxonomy.Id, root.Id, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        
        // Corrupt initial values
        root.Lft = 0; root.Rgt = 0;
        child.Lft = 0; child.Rgt = 0;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, child);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _service.RebuildHierarchyAsync(taxonomy.Id, ct: ct);

        // Assert (Weak)
        result.IsSuccess.Should().BeTrue();

        // Assert (Strong)
        _dbContext.ChangeTracker.Clear();
        var updatedRoot = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == root.Id, ct);
        var updatedChild = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == child.Id, ct);

        updatedRoot.Lft.Should().Be(1);
        updatedRoot.Rgt.Should().Be(4);
        updatedRoot.Permalink.Should().Be("categories");

        updatedChild.Lft.Should().Be(2);
        updatedChild.Rgt.Should().Be(3);
        updatedChild.Permalink.Should().Be("categories/shirts");
    }

    [Fact(DisplayName = "RebuildHierarchy: Should correctly update subtree and shift others")]
    public async Task RebuildHierarchy_ShouldCorrectlyUpdate_Subtree()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Cat", "Cat", null, 0, "cat", null, null, null, false, null, null, false, null, null).Value;
        var branch1 = TaxonMethod.Create(taxonomy.Id, root.Id, "B1", "B1", null, 0, "b1", null, null, null, false, null, null, false, null, null).Value;
        var branch2 = TaxonMethod.Create(taxonomy.Id, root.Id, "B2", "B2", null, 1, "b2", null, null, null, false, null, null, false, null, null).Value;
        
        // Setup initial healthy state: root(1,6), B1(2,3), B2(4,5)
        root.Lft = 1; root.Rgt = 6;
        branch1.Lft = 2; branch1.Rgt = 3;
        branch2.Lft = 4; branch2.Rgt = 5;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, branch1, branch2);
        await _dbContext.SaveChangesAsync(ct);

        // Now add a child to B1 but keep B1's Rgt as 3 (corrupting it)
        var leaf = TaxonMethod.Create(taxonomy.Id, branch1.Id, "Leaf", "Leaf", null, 0, "leaf", null, null, null, false, null, null, false, null, null).Value;
        _dbContext.Set<Taxon>().Add(leaf);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act: Rebuild subtree starting from B1
        var result = await _service.RebuildHierarchyAsync(taxonomy.Id, branch1.Id, ct: ct);

        // Assert (Weak)
        result.IsSuccess.Should().BeTrue();

        // Assert (Strong)
        _dbContext.ChangeTracker.Clear();
        var updatedRoot = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == root.Id, ct);
        var updatedB1 = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == branch1.Id, ct);
        var updatedLeaf = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == leaf.Id, ct);
        var updatedB2 = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == branch2.Id, ct);

        // B1 should now be (2,5), Leaf (3,4)
        updatedB1.Lft.Should().Be(2);
        updatedB1.Rgt.Should().Be(5);
        updatedLeaf.Lft.Should().Be(3);
        updatedLeaf.Rgt.Should().Be(4);

        // B2 and Root should have shifted/expanded
        updatedB2.Lft.Should().Be(6); // shifted by +2
        updatedB2.Rgt.Should().Be(7);
        updatedRoot.Rgt.Should().Be(8);
    }

    [Fact(DisplayName = "RebuildNestedSets: Should only update coordinates")]
    public async Task RebuildNestedSets_ShouldOnlyUpdateCoordinates()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Cat", "Cat", null, 0, "cat", null, null, null, false, null, null, false, null, null).Value;
        
        root.Lft = 0; root.Rgt = 0;
        root.Permalink = "old-permalink";

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(root);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _service.RebuildNestedSetsAsync(taxonomy.Id, ct: ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _dbContext.ChangeTracker.Clear();
        var updatedRoot = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == root.Id, ct);
        updatedRoot.Lft.Should().Be(1);
        updatedRoot.Rgt.Should().Be(2);
        updatedRoot.Permalink.Should().Be("old-permalink"); // Should NOT change
    }

    [Fact(DisplayName = "RebuildNestedSetsInternal: Should correctly rebuild coordinates in-memory")]
    public void RebuildNestedSetsInternal_ShouldRebuildCoordinates()
    {
        // Arrange
        var taxoId = Guid.NewGuid();
        var root = TaxonMethod.Create(taxoId, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var child = TaxonMethod.Create(taxoId, root.Id, "Child", "Child", null, 0, "child", null, null, null, false, null, null, false, null, null).Value;
        
        var taxons = new List<Taxon> { root, child };

        // Act
        var result = InvokePrivateStaticMethod<Result>("RebuildNestedSetsInternal", taxons, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        root.Lft.Should().Be(1);
        root.Rgt.Should().Be(4);
        child.Lft.Should().Be(2);
        child.Rgt.Should().Be(3);
    }

    [Fact(DisplayName = "RebuildHierarchy: Should handle single root with no children")]
    public async Task RebuildHierarchy_ShouldHandleSingleRoot()
    {
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Categories", "Categories", null, 0, "categories", null, null, null, false, null, null, false, null, null).Value;
        
        root.Lft = 0; root.Rgt = 0;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(root);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        var result = await _service.RebuildHierarchyAsync(taxonomy.Id, ct: ct);

        result.IsSuccess.Should().BeTrue();

        _dbContext.ChangeTracker.Clear();
        var updated = await _dbContext.Set<Taxon>().AsNoTracking().FirstAsync(x => x.Id == root.Id, ct);
        updated.Lft.Should().Be(1);
        updated.Rgt.Should().Be(2);
        updated.Permalink.Should().Be("categories");
    }
}
